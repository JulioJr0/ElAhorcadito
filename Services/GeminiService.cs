using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace ElAhorcadito.Services
{
    public class GeminiService : IGeminiService
    {
        public IConfiguration Configuration { get; }
        public HttpClient HttpClient { get; }

        public GeminiService(IConfiguration configuration)
        {
            Configuration = configuration;
            HttpClient = new HttpClient();
        }

        public async Task<(string tema, string descripcion, List<string> palabras)> GenerarTemaYPalabras()
        {
            var apiKey = Configuration["GeminiSettings:ApiKey"];
            var apiUrl = $"{Configuration["GeminiSettings:ApiUrl"]}?key={apiKey}";

            var prompt = @"Genera un tema aleatorio interesante para un juego de ahorcado, una descripción breve del tema (máximo 100 caracteres) y 10 palabras relacionadas. 
            Las palabras deben tener entre 5 y 12 letras, ser en español y estar en mayúsculas. 
            Responde ÚNICAMENTE en formato JSON válido sin markdown, con esta estructura exacta:
            {
              ""tema"": ""Nombre del tema"",
              ""descripcion"": ""Descripción corta del tema"",
              ""palabras"": [""PALABRA1"", ""PALABRA2"", ""PALABRA3"", ""PALABRA4"", ""PALABRA5"", ""PALABRA6"", ""PALABRA7"", ""PALABRA8"", ""PALABRA9"", ""PALABRA10""]
            }";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await HttpClient.PostAsync(apiUrl, content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var jsonResponse = JsonDocument.Parse(responseBody);

                var textResponse = jsonResponse.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? "";

                textResponse = textResponse.Replace("```json", "").Replace("```", "").Trim();

                var resultado = JsonSerializer.Deserialize<JsonElement>(textResponse);
                var tema = resultado.GetProperty("tema").GetString() ?? "Tema Aleatorio";
                var descripcion = resultado.GetProperty("descripcion").GetString() ?? ""; //
                var palabras = resultado.GetProperty("palabras")
                    .EnumerateArray()
                    .Select(p => p.GetString()?.ToUpper() ?? "")
                    .Where(p => !string.IsNullOrEmpty(p))
                    .Take(10)
                    .ToList();

                return (tema, descripcion, palabras);//
            }
            catch (Exception)
            {
                return ("Tema de Ejemplo", "Descripción de ejemplo", new List<string> //
                {
                    "PROGRAMACION", "JAVASCRIPT", "PYTHON", "DATABASE",
                    "SERVIDOR", "FRONTEND", "BACKEND", "ALGORITMO",
                    "VARIABLE", "FUNCION"
                });
            }
        }
    }
}
