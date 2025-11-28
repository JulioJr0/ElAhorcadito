namespace ElAhorcadito.Models.DTOs.Juego
{
    public class RetoDiarioDTO
    {
        public int IdTema { get; set; }
        public string NombreTema { get; set; } = null!;
        public int PalabraActualIndex { get; set; }
        public int PalabrasCompletadas { get; set; }
        public int TotalPalabras {  get; set; }
        public DateTime FechaLimite { get; set; }
        public bool YaCompletadoHoy { get; set; }
        public string PalabraAdivinar { get; set; } = null!;
    }
}
