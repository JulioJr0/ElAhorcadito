using ElAhorcadito.Models.DTOs.Push;
using ElAhorcadito.Models.Entities;
using ElAhorcadito.Repositories;
using System.Text.Json;
using WebPush;

namespace ElAhorcadito.Services
{
    public class PushNotificationService:IPushNotificationService
    {
        private readonly VapidDetails vapid;
        public IRepository<PushSubscriptions> Repository { get; }
        public IConfiguration Configuration { get; }

        public PushNotificationService(IRepository<PushSubscriptions> repository,
            IConfiguration configuration)
        {
            Repository = repository;
            Configuration = configuration;
            vapid = new VapidDetails
            {
                Subject = configuration["VAPID:subject"] ?? "",
                PublicKey = configuration["VAPID:publicKey"] ?? "",
                PrivateKey = configuration["VAPID:privateKey"] ?? ""
            };
        }
        public string GetPublicKey()
        {
            return vapid.PublicKey;
        }
        public void Suscribir(SubscriptionDTO dto, int idUsuario)
        {
            var existente = Repository.GetAll()
                .FirstOrDefault(x => x.Endpoint == dto.Endpoint && x.IdUsuario == idUsuario);

            if (existente == null)
            {
                var suscripcion = new PushSubscriptions
                {
                    IdUsuario = idUsuario,
                    Endpoint = dto.Endpoint,
                    P256dh = dto.Keys.P256dh,
                    Auth = dto.Keys.Auth,
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow
                };
                Repository.Insert(suscripcion);
                Console.WriteLine($"[Push] ✅ Suscripción guardada para usuario {idUsuario}");
            }
            else if (existente.Activo == false)
            {
                existente.Activo = true;
                existente.P256dh = dto.Keys.P256dh;
                existente.Auth = dto.Keys.Auth;
                Repository.Update(existente);
                Console.WriteLine($"[Push] ✅ Suscripción reactivada para usuario {idUsuario}");
            }
            else
            {
                Console.WriteLine($"[Push] ℹ️ Suscripción ya existe para usuario {idUsuario}");
            }
        }
        public void Desuscribir(string endpoint, int idUsuario)
        {
            var suscripcion = Repository.GetAll()
                .FirstOrDefault(x => x.Endpoint == endpoint && x.IdUsuario == idUsuario);

            if (suscripcion != null)
            {
                suscripcion.Activo = false;
                Repository.Update(suscripcion);
            }
        }
        public async Task EnviarNotificacionPersonal(int idUsuario, string titulo, string mensaje)
        {
            var suscripciones = Repository.GetAll()
                .Where(x => x.IdUsuario == idUsuario && x.Activo == true)
                .ToList();

            await EnviarADestinatarios(suscripciones, titulo, mensaje);
        }
        public async Task EnviarNotificacionGlobal(string titulo, string mensaje)
        {
            var suscripciones = Repository.GetAll()
                .Where(x => x.Activo == true)
                .ToList();

            await EnviarADestinatarios(suscripciones, titulo, mensaje);
        }
        private async Task EnviarADestinatarios(
            List<PushSubscriptions> destinatarios,
            string titulo,
            string mensaje)
        {
            var cliente = new WebPushClient();

            foreach (var destinatario in destinatarios)
            {
                try
                {
                    var subscription = new PushSubscription(
                        destinatario.Endpoint,
                        destinatario.P256dh,
                        destinatario.Auth
                    );

                    var payload = new
                    {
                        titulo = titulo,
                        mensaje = mensaje,
                        icono = "/images/icono-192.png",
                        badge = "/images/badge-72.png"
                    };

                    await cliente.SendNotificationAsync(
                        subscription,
                        JsonSerializer.Serialize(payload),
                        vapid
                    );

                    destinatario.FechaUltimaNotificacion = DateTime.UtcNow;
                    Repository.Update(destinatario);
                }
                catch (WebPushException ex)
                {
                    // Si la suscripción ya no existe (410 Gone), desactivarla
                    if (ex.StatusCode == System.Net.HttpStatusCode.Gone)
                    {
                        destinatario.Activo = false;
                        Repository.Update(destinatario);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error enviando notificación: {ex.Message}");
                }
            }
        }
    }
}
