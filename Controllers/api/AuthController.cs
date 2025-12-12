using ElAhorcadito.Models.DTOs.Auth;
using ElAhorcadito.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ElAhorcadito.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public IAuthService Service { get; }
        public ITemaService TemaService { get; }
        public IBackgroundTaskQueue TaskQueue { get; }
        public IServiceScopeFactory ScopeFactory { get; }

        public AuthController(IAuthService service, ITemaService temaService, IBackgroundTaskQueue taskQueue, IServiceScopeFactory scopeFactory)
        {
            Service = service;
            TemaService = temaService;
            TaskQueue = taskQueue;
            ScopeFactory = scopeFactory;
        }

        [HttpPost("register")]//ok
        public IActionResult Register(RegistroDTO dto)
        {
            Service.RegistrarUsuario(dto);
            return Ok();
        }

        [HttpPost("login")]//ok
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            var (token, refreshToken) = Service.IniciarSesion(dto);
            if (token == string.Empty)
            {
                return BadRequest("Correo electrónico o contraseña incorrecta");
            }
            HttpContext.Response.Cookies.Append("refreshtoken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax
            });

            //ENCOLAR TAREA EN BACKGROUND CON SCOPE
            await TaskQueue.QueueBackgroundWorkItemAsync(async token =>
            {
                using (var scope = ScopeFactory.CreateScope())
                {
                    var temaService = scope.ServiceProvider.GetRequiredService<ITemaService>();

                    try
                    {
                        await temaService.CrearTemasFaltantes();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error creando temas en background: {ex.Message}");
                    }
                }
            });

            return Ok(token); 
        }

        [HttpGet("renew")]
        public IActionResult Renew()
        {
            var cookie = Request.Cookies["refreshtoken"];

            if (cookie != null)
            {
                var (newToken, newRefreshToken) = Service.RefreshToken(cookie);

                if (newToken == string.Empty)
                {
                    return Unauthorized();
                }

                HttpContext.Response.Cookies.Append("refreshtoken", newRefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false,
                    SameSite = SameSiteMode.Lax
                });

                return Ok(newToken);
            }

            return Unauthorized();
        }

        [Authorize]
        [HttpGet("perfil")]//ok
        public IActionResult GetPerfil()
        {
            if (!int.TryParse(User.FindFirst("Id")?.Value, out int idUsuario))
                return Unauthorized();

            var perfil = Service.GetPerfil(idUsuario);
            if (perfil == null)
                return NotFound();

            return Ok(perfil);
        }

        [Authorize]
        [HttpPut("preferencias")]//ok
        public IActionResult ActualizarPreferencias(PreferenciasDTO dto)
        {
            if (!int.TryParse(User.FindFirst("Id")?.Value, out int idUsuario))
                return Unauthorized();

            Service.ActualizarPreferencias(idUsuario, dto);
            return Ok();
        }

        [Authorize]
        [HttpPost("reiniciar-progreso")]
        public IActionResult ReiniciarProgreso()
        {
            if (!int.TryParse(User.FindFirst("Id")?.Value, out int idUsuario))
                return Unauthorized();

            try
            {
                Service.ReiniciarProgreso(idUsuario);
                return Ok(new { mensaje = "Progreso reiniciado correctamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}
