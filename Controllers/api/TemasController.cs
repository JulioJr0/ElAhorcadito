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
    }
}
