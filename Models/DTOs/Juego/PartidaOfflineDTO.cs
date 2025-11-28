namespace ElAhorcadito.Models.DTOs.Juego
{
    public class PartidaOfflineDTO : IPartidaOfflineDTO
    {
        public int IdTema { get; set; }
        public int PalabraIndex { get; set; }
        public bool Ganada { get; set; }
        public DateTime FechaJuego { get; set; }
    }
}
