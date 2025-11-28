namespace ElAhorcadito.Models.DTOs.Juego
{
    public interface IPartidaOfflineDTO
    {
        int IdTema { get; set; }
        int PalabraIndex { get; set; }
        bool Ganada { get; set; }
        DateTime FechaJuego { get; set; }
    }
}
