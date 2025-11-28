using ElAhorcadito.Models.DTOs.Estadisticas;

namespace ElAhorcadito.Services
{
    public interface IRachaService
    {
        void CheckAndExtendRacha(int idUsuario, int idTemaDiario);
        EstadisticasDTO GetEstadisticas(int idUsuario);
    }
}
