namespace ElAhorcadito.Models.DTOs.Temas
{
    public interface ITemaListaDTO
    {
        int IdTema { get; set; }
        string Nombre { get; set; }
        string Descripcion { get; set; }
        DateTime FechaGeneracion { get; set; }
        int PalabrasCompletadas { get; set; }
        int TotalPalabras {  get; set; }
        double PorcentajeProgreso { get; set; }
    }
}
