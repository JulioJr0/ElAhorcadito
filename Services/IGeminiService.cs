namespace ElAhorcadito.Services
{
    public interface IGeminiService
    {
        Task<(string tema, string descripcion, List<string> palabras)> GenerarTemaYPalabras();
    }
}
