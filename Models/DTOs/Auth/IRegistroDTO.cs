namespace ElAhorcadito.Models.DTOs.Auth
{
    public interface IRegistroDTO
    {
        string NombreUsuario { get; set; }
        string Email { get; set; }
        string Password { get; set; }
    }
}
