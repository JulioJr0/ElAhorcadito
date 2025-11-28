namespace ElAhorcadito.Models.DTOs.Auth
{
    public interface IPerfilDTO
    {
        string NombreUsuario { get; set; }
        string Email { get; set; }
        bool SonidosActivados { get; set; }
        bool RecordatorioDiario { get; set; } 
    }
}
