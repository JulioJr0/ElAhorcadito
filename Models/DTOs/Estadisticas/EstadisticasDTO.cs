namespace ElAhorcadito.Models.DTOs.Estadisticas
{
    public class EstadisticasDTO : IEstadisticasDTO
    {
        public int DiasConsecutivos { get; set; }
        public int PalabrasTotales { get; set; }
        public int TemasCompletados { get; set; }
        public Dictionary<string, bool> ProgresoSemanal { get; set; } = new Dictionary<string, bool>();
    }
}
