namespace ElAhorcadito.Models.DTOs.Juego
{
    public class ActualizarProgresoDTO : IActualizarProgresoDTO
    {
        public int IdTema { get; set; }
        public int PalabraActualIndex { get; set; }
        public string Resultado { get; set; } = null!; // "GANADA" o "PERDIDA"
    }
}
