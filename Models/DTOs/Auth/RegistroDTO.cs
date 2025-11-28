namespace ElAhorcadito.Models.DTOs.Auth
{
    public class RegistroDTO : IRegistroDTO
    {
        public string NombreUsuario { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
