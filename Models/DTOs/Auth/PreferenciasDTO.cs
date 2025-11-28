namespace ElAhorcadito.Models.DTOs.Auth
{
    public class PreferenciasDTO : IPreferenciasDTO
    {
        public bool SonidosActivados {  get; set; }
        public bool RecordatorioDiario { get; set; }
    }
}
