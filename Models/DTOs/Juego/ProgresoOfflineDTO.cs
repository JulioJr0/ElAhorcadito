namespace ElAhorcadito.Models.DTOs.Juego
{
    public class ProgresoOfflineDTO : IProgresoOfflineDTO
    {
        public List<IPartidaOfflineDTO> Partidas { get; set; } = new List<IPartidaOfflineDTO>();
    }
}
