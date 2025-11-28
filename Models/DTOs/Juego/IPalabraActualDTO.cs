namespace ElAhorcadito.Models.DTOs.Juego
{
    public interface IPalabraActualDTO
    {
        int PalabraIndex { get; set; }
        string Palabra {  get; set; }
        int PalabrasCompletadas { get; set; }
        int TotalPalabras {  get; set; }
    }
}
