namespace ElAhorcadito.Models.DTOs.Temas
{
    public class TemaListaDTO : ITemaListaDTO
    {
        public int IdTema { get; set; }
        public string Nombre { get; set; } = null!;
        public string Descripcion {  get; set; } = null!;
        public DateTime FechaGeneracion { get; set; }
        public int PalabrasCompletadas { get; set; }
        public int TotalPalabras { get; set; }
        public double PorcentajeProgreso { get; set; }
    }
}
