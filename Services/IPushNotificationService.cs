using ElAhorcadito.Models.DTOs.Push;

namespace ElAhorcadito.Services
{
    public interface IPushNotificationService
    {
        void Suscribir(SubscriptionDTO dto, int idUsuario);
        void Desuscribir(string endpoint, int idUsuario);
        string GetPublicKey();
        Task EnviarNotificacionPersonal(int idUsuario, string titulo, string mensaje);
        Task EnviarNotificacionGlobal(string titulo, string mensaje);
    }
}
