namespace ElAhorcadito.Models.DTOs.Juego
{
    public interface IProgresoOfflineDTO
    {
        List<IPartidaOfflineDTO> Partidas { get; set; }
    }
}
