namespace ElAhorcadito.Models.DTOs.Auth
{
    public class LoginDTO : ILoginDTO
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
