using ElAhorcadito.Models.DTOs.Push;
using ElAhorcadito.Models.Entities;
using ElAhorcadito.Repositories;
using ElAhorcadito.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElAhorcadito.Controllers.api
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificacionesController : ControllerBase
    {
        public IPushNotificationService PushService { get; }
        public IRepository<Notificaciones> NotificacionesRepository { get; }

        public NotificacionesController(IPushNotificationService pushService,
            IRepository<Notificaciones> notificacionesRepository)
        {
            PushService = pushService;
            NotificacionesRepository = notificacionesRepository;
        }

        [HttpGet("publickey")]
        public IActionResult GetPublicKey()
        {
            return Ok(PushService.GetPublicKey());
        }

        [Authorize]
        [HttpPost("suscribir")]
        public IActionResult Suscribir(SubscriptionDTO dto)
        {
            if (!int.TryParse(User.FindFirst("Id")?.Value, out int idUsuario))
                return Unauthorized();

            // Validar que lleguen los datos
            if (string.IsNullOrEmpty(dto?.Endpoint))
            {
                return BadRequest(new { error = "Endpoint es requerido" });
            }

            if (dto.Keys == null || string.IsNullOrEmpty(dto.Keys.P256dh) || string.IsNullOrEmpty(dto.Keys.Auth))
            {
                return BadRequest(new { error = "Keys (p256dh y auth) son requeridas" });
            }

            try
            {
                PushService.Suscribir(dto, idUsuario);
                return Ok(new { mensaje = "Suscripción registrada correctamente" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Push Controller] Error: {ex.Message}");
                Console.WriteLine($"[Push Controller] Stack: {ex.StackTrace}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("desuscribir")]
        public IActionResult Desuscribir([FromBody] string endpoint)
        {
            if (!int.TryParse(User.FindFirst("Id")?.Value, out int idUsuario))
                return Unauthorized();

            PushService.Desuscribir(endpoint, idUsuario);
            return Ok(new { mensaje = "Desuscripción registrada" });
        }

        [Authorize]
        [HttpGet("listar")]
        public IActionResult ListarNotificaciones()
        {
            if (!int.TryParse(User.FindFirst("Id")?.Value, out int idUsuario))
                return Unauthorized();
            //lógica para obtener notificaciones del usuario
            var notificaciones = NotificacionesRepository.GetAll()
                .Where(n => n.IdUsuario == idUsuario)
                .OrderByDescending(n => n.FechaCreacion)
                .Take(50) // Últimas 50 notificaciones
                .Select(n => new
                {
                    id = n.Id,
                    tipo = n.TipoNotificacion,
                    titulo = n.Titulo,
                    mensaje = n.Mensaje,
                    fechaCreacion = n.FechaCreacion,
                    leida = n.Leida ?? false
                })
                .ToList();

            return Ok(notificaciones);
        }

        [Authorize]
        [HttpPut("marcar-leida/{id}")]
        public IActionResult MarcarComoLeida(int id)
        {
            if (!int.TryParse(User.FindFirst("Id")?.Value, out int idUsuario))
                return Unauthorized();

            var notificacion = NotificacionesRepository.Get(id);

            if (notificacion == null)
                return NotFound(new { mensaje = "Notificación no encontrada" });

            // Verificar que la notificación pertenezca al usuario
            if (notificacion.IdUsuario != idUsuario)
                return Forbid();

            notificacion.Leida = true;
            NotificacionesRepository.Update(notificacion);

            return Ok(new { mensaje = "Notificación marcada como leída" });
        }

        [Authorize]
        [HttpPut("marcar-todas-leidas")]
        public IActionResult MarcarTodasLeidas()
        {
            if (!int.TryParse(User.FindFirst("Id")?.Value, out int idUsuario))
                return Unauthorized();

            var notificacionesNoLeidas = NotificacionesRepository.GetAll()
                .Where(n => n.IdUsuario == idUsuario && n.Leida == false)
                .ToList();

            foreach (var notificacion in notificacionesNoLeidas)
            {
                notificacion.Leida = true;
                NotificacionesRepository.Update(notificacion);
            }

            return Ok(new { mensaje = "Todas las notificaciones marcadas como leídas" });
        }

        [Authorize]
        [HttpDelete("eliminar/{id}")]
        public IActionResult EliminarNotificacion(int id)
        {
            if (!int.TryParse(User.FindFirst("Id")?.Value, out int idUsuario))
                return Unauthorized();

            var notificacion = NotificacionesRepository.Get(id);

            if (notificacion == null)
                return NotFound(new { mensaje = "Notificación no encontrada" });

            if (notificacion.IdUsuario != idUsuario)
                return Forbid();

            NotificacionesRepository.Delete(id);

            return Ok(new { mensaje = "Notificación eliminada" });
        }

        [Authorize]
        [HttpDelete("eliminar-todas")]
        public IActionResult EliminarTodas()
        {
            if (!int.TryParse(User.FindFirst("Id")?.Value, out int idUsuario))
                return Unauthorized();

            var notificaciones = NotificacionesRepository.GetAll()
                .Where(n => n.IdUsuario == idUsuario)
                .ToList();

            foreach (var notificacion in notificaciones)
            {
                NotificacionesRepository.Delete(notificacion.Id);
            }

            return Ok(new { mensaje = "Todas las notificaciones eliminadas" });
        }

        [Authorize]
        [HttpGet("contar-no-leidas")]
        public IActionResult ContarNoLeidas()
        {
            if (!int.TryParse(User.FindFirst("Id")?.Value, out int idUsuario))
                return Unauthorized();

            var count = NotificacionesRepository.GetAll()
                .Count(n => n.IdUsuario == idUsuario && n.Leida == false);

            return Ok(new { noLeidas = count });
        }

    }
}