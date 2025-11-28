using ElAhorcadito.Models.DTOs.Juego;

namespace ElAhorcadito.Services
{
    public interface IJuegoService
    {
        Task<RetoDiarioDTO> IniciarRetoDiario(int idUsuario);
        PalabraActualDTO? ObtenerPalabraActual(int idUsuario, int idTema);
        void ActualizarProgresoPalabra(int idUsuario, ActualizarProgresoDTO dto);
        Task SincronizarProgresoOffline(int idUsuario, ProgresoOfflineDTO dto);
    }
}
