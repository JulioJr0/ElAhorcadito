namespace ElAhorcadito.Models.DTOs.Juego
{
    public class PalabraActualDTO : IPalabraActualDTO
    {
        public int PalabraIndex { get; set; }
        public string Palabra { get; set; } = null!;
        public int PalabrasCompletadas { get; set; }
        public int TotalPalabras { get; set; }
    }
}
