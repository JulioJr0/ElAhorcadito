namespace ElAhorcadito.Models.DTOs.Juego
{
    public interface IActualizarProgresoDTO
    {
        int IdTema {  get; set; }
        int PalabraActualIndex { get; set; }
        string Resultado { get; set; } // "GANADA" o "PERDIDA"
    }
}
