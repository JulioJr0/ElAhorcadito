using ElAhorcadito.Models.Entities;

namespace ElAhorcadito.Services
{
    public interface ITemaService
    {
        Task<Temas> GetOrCreateTemaDiario();
        IEnumerable<object> ObtenerPaginaTemas(int pageNumber, int idUsuario);
    }
}
