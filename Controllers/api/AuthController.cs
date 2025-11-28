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

        public AuthController(IAuthService service)
        {
            Service = service;
        }

        [HttpPost("register")]//ok
        public IActionResult Register(RegistroDTO dto)
        {
            Service.RegistrarUsuario(dto);
            return Ok();
        }

        [HttpPost("login")]//ok
        public IActionResult Login(LoginDTO dto)
        {
            var (token, refreshToken) = Service.IniciarSesion(dto);
            if (token == string.Empty)
            {
                return BadRequest("Correo electrónico o contraseña incorrecta");
            }
            return Ok(new { token, refreshToken });
        }

        [HttpPost("refresh-token")]
        public IActionResult RefreshToken([FromBody] string refreshToken)
        {
            var (newToken, newRefreshToken) = Service.RefreshToken(refreshToken);
            if (newToken == string.Empty)
            {
                return Unauthorized("Refresh token inválido o expirado");
            }
            return Ok(new { token = newToken, refreshToken = newRefreshToken });
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





    }
}
