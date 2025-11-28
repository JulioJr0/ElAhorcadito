namespace ElAhorcadito.Models.DTOs.Auth
{
    public interface ILoginDTO
    {
        string Email { get; set; }
        string Password { get; set; }
    }
}
