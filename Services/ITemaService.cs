using ElAhorcadito.Models.Entities;

namespace ElAhorcadito.Services
{
    public interface ITemaService
    {
        Task<Temas> GetOrCreateTemaDiario();
        IEnumerable<object> ObtenerPaginaTemas(int pageNumber, int idUsuario);
        Task CrearTemasFaltantes(DateTime? fechaInicio = null);

        // ✅ NUEVA FIRMA
        object? ObtenerTemaPorId(int idTema, int idUsuario);
    }
}
