namespace ElAhorcadito.Models.DTOs.Estadisticas
{
    public interface IEstadisticasDTO
    {
        int DiasConsecutivos { get; set; }
        int PalabrasTotales { get; set; }
        int TemasCompletados { get; set; }
        int TemasTotales { get; set; }
        Dictionary<string, bool> ProgresoSemanal {  get; set; }
    }
}
