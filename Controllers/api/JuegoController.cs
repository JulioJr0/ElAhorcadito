using ElAhorcadito.Models.DTOs.Juego;
using ElAhorcadito.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ElAhorcadito.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class JuegoController : ControllerBase
    {
        public IJuegoService JuegoService { get; }

        public JuegoController(IJuegoService juegoService)
        {
            JuegoService = juegoService;
        }

        [HttpGet("iniciar-diario")]
        public async Task<IActionResult> IniciarRetoDiario()
        {
            if (!int.TryParse(User.FindFirst("Id")?.Value, out int idUsuario))
                return Unauthorized();

            var reto = await JuegoService.IniciarRetoDiario(idUsuario);
            return Ok(reto);
        }

        [HttpGet("palabra-actual/{idTema}")]
        public IActionResult ObtenerPalabraActual(int idTema)
        {
            if (!int.TryParse(User.FindFirst("Id")?.Value, out int idUsuario))
                return Unauthorized();

            var palabra = JuegoService.ObtenerPalabraActual(idUsuario, idTema);
            if (palabra == null)
                return NotFound("Ya completaste todas las palabras de este tema");

            return Ok(palabra);
        }

        [HttpPost("actualizar-progreso")]
        public IActionResult ActualizarProgreso(ActualizarProgresoDTO dto)
        {
            if (!int.TryParse(User.FindFirst("Id")?.Value, out int idUsuario))
                return Unauthorized();

            JuegoService.ActualizarProgresoPalabra(idUsuario, dto);
            return Ok(new { mensaje = "Progreso actualizado correctamente" });
        }

        [HttpPost("sincronizar")]
        public async Task<IActionResult> SincronizarOffline(ProgresoOfflineDTO dto)
        {
            if (!int.TryParse(User.FindFirst("Id")?.Value, out int idUsuario))
                return Unauthorized();

            await JuegoService.SincronizarProgresoOffline(idUsuario, dto);
            return Ok(new { mensaje = "Progreso sincronizado" });
        }
    }
}
