using ElAhorcadito.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ElAhorcadito.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TemasController : ControllerBase
    {
        public ITemaService TemaService { get; }

        public TemasController(ITemaService temaService)
        {
            TemaService = temaService;
        }

        [HttpGet("pagina/{pageNumber}")]
        public IActionResult GetPage(int pageNumber)
        {
            if (!int.TryParse(User.FindFirst("Id")?.Value, out int idUsuario))
                return Unauthorized();

            var temas = TemaService.ObtenerPaginaTemas(pageNumber, idUsuario);
            return Ok(temas);
        }

        // ✅ NUEVO ENDPOINT: Obtener tema individual por ID
        [HttpGet("{idTema}")]
        public IActionResult GetTemaById(int idTema)
        {
            if (!int.TryParse(User.FindFirst("Id")?.Value, out int idUsuario))
                return Unauthorized();

            var tema = TemaService.ObtenerTemaPorId(idTema, idUsuario);

            if (tema == null)
                return NotFound(new { mensaje = "Tema no encontrado" });

            return Ok(tema);
        }

        [HttpGet("verificar-reinicio")]
        public IActionResult VerificarReinicio()
        {
            if (!int.TryParse(User.FindFirst("Id")?.Value, out int idUsuario))
                return Unauthorized();

            var temas = TemaService.ObtenerPaginaTemas(1, idUsuario);

            // Verificar si TODOS los temas tienen progreso en 0
            bool todasEnCero = temas.All(t =>
            {
                var progreso = (dynamic)t;
                return progreso.PalabrasCompletadas == 0;
            });

            return Ok(new { todasEnCero });
        }
    }
}
