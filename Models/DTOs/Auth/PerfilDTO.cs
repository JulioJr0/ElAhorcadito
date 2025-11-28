namespace ElAhorcadito.Models.DTOs.Auth
{
    public class PerfilDTO : IPerfilDTO
    {
        public string NombreUsuario { get; set; } = null!;
        public string Email { get; set; } = null!;
        public bool SonidosActivados { get; set; } 
        public bool RecordatorioDiario {  get; set; }
    }
}
