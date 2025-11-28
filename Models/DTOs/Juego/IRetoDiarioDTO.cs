namespace ElAhorcadito.Models.DTOs.Juego
{
    public interface IRetoDiarioDTO
    {
        int IdTema { get; set; }
        string NombreTema { get; set; }
        int PalabraActualIndex { get; set; }
        int PalabrasCompletadas { get; set; }
        int TotalPalabras {  get; set; }
        DateTime FechaLimite { get; set; }
        bool YaCompletadoHoy { get; set; }
        string PalabraAdivinar { get; set; }
    }
}
