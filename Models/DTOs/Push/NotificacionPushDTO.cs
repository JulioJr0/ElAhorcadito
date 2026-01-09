namespace ElAhorcadito.Models.DTOs.Push
{
    public class NotificacionPushDTO
    {
        public string Titulo { get; set; } = null!;
        public string Mensaje { get; set; } = null!;
        public string? Icono { get; set; }
        public string? Badge { get; set; }
        public string? Url { get; set; }
    }
}
