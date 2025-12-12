using ElAhorcadito.Models.DTOs.Auth;

namespace ElAhorcadito.Services
{
    public interface IAuthService
    {
        void RegistrarUsuario(IRegistroDTO dto);
        (string token, string refreshToken) IniciarSesion(ILoginDTO dto);
        (string token, string refreshToken) RefreshToken(string refreshToken);
        IPerfilDTO? GetPerfil(int idUsuario);
        void ActualizarPreferencias(int idUsuario, PreferenciasDTO dto);
        void ReiniciarProgreso(int idUsuario);
    }
}
