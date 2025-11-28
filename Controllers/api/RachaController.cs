using ElAhorcadito.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ElAhorcadito.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RachaController : ControllerBase
    {
        public IRachaService RachaService { get; }

        public RachaController(IRachaService rachaService)
        {
            RachaService = rachaService;
        }

        [HttpGet("estadisticas")]
        public IActionResult GetEstadisticas()
        {
            if (!int.TryParse(User.FindFirst("Id")?.Value, out int idUsuario))
                return Unauthorized();

            var estadisticas = RachaService.GetEstadisticas(idUsuario);
            return Ok(estadisticas);
        }
    }
}
