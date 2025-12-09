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

            //var apiUrl = $"https://generativelanguage.googleapis.com/v1/models/gemini-1.5-flash:generateContent?key={apiKey}";
            //var apiUrl = $"https://generativelanguage.googleapis.com/v1/models/gemini-pro:generateContent?key={apiKey}";

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
                },
                generationConfig = new
                {
                    temperature = 0.9,
                    topK = 1,
                    topP = 1,
                    maxOutputTokens = 2048,
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                Console.WriteLine("🔄 Llamando a Gemini API...");

                var response = await HttpClient.PostAsync(apiUrl, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                // ✅ MOSTRAR RESPUESTA COMPLETA PARA DEBUG
                Console.WriteLine("=== RESPUESTA DE GEMINI ===");
                Console.WriteLine(responseBody);
                Console.WriteLine("===========================");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ Error HTTP {response.StatusCode}: {responseBody}");
                    return GenerarTemaRespaldo();
                }

                var jsonResponse = JsonDocument.Parse(responseBody);

                // ✅ VERIFICAR SI HAY CONTENIDO EN LA RESPUESTA
                if (!jsonResponse.RootElement.TryGetProperty("candidates", out var candidates) ||
                    candidates.GetArrayLength() == 0)
                {
                    Console.WriteLine("❌ Gemini no devolvió candidatos");
                    return GenerarTemaRespaldo();
                }

                var textResponse = candidates[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? "";

                Console.WriteLine($"📝 Texto recibido: {textResponse.Substring(0, Math.Min(200, textResponse.Length))}...");

                // ✅ LIMPIAR RESPUESTA (quitar markdown si viene)
                textResponse = textResponse
                    .Replace("```json", "")
                    .Replace("```", "")
                    .Trim();

                // ✅ PARSEAR JSON
                var resultado = JsonSerializer.Deserialize<JsonElement>(textResponse);

                if (!resultado.TryGetProperty("tema", out var temaElement) ||
                    !resultado.TryGetProperty("descripcion", out var descripcionElement) ||
                    !resultado.TryGetProperty("palabras", out var palabrasElement))
                {
                    Console.WriteLine("❌ JSON incompleto, faltan propiedades requeridas");
                    return GenerarTemaRespaldo();
                }

                var tema = temaElement.GetString() ?? "Tema Aleatorio";
                var descripcion = descripcionElement.GetString() ?? "";
                var palabras = palabrasElement
                    .EnumerateArray()
                    .Select(p => p.GetString()?.ToUpper() ?? "")
                    .Where(p => !string.IsNullOrEmpty(p) && p.Length >= 5 && p.Length <= 12)
                    .Take(10)
                    .ToList();

                // ✅ VALIDAR QUE TENEMOS 10 PALABRAS
                if (palabras.Count < 10)
                {
                    Console.WriteLine($"⚠️ Solo se recibieron {palabras.Count} palabras válidas, usando respaldo");
                    return GenerarTemaRespaldo();
                }

                Console.WriteLine($"✅ Tema generado: {tema} con {palabras.Count} palabras");
                return (tema, descripcion, palabras);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"❌ Error de conexión con Gemini: {ex.Message}");
                return GenerarTemaRespaldo();
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"❌ Error parseando JSON de Gemini: {ex.Message}");
                return GenerarTemaRespaldo();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error inesperado en Gemini: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return GenerarTemaRespaldo();
            }
        }

        // ✅ MÉTODO AUXILIAR: Generar tema de respaldo
        private (string tema, string descripcion, List<string> palabras) GenerarTemaRespaldo()
        {
            Console.WriteLine("🔄 Generando tema de respaldo...");

            var temas = new[]
            {
                new
                {
                    tema = "Programación",
                    descripcion = "Conceptos fundamentales de desarrollo de software",
                    palabras = new List<string> { "JAVASCRIPT", "PYTHON", "DATABASE", "SERVIDOR", "FRONTEND", "BACKEND", "ALGORITMO", "VARIABLE", "FUNCION", "SINTAXIS" }
                },
                new
                {
                    tema = "Animales Salvajes",
                    descripcion = "Criaturas fascinantes del reino animal",
                    palabras = new List<string> { "ELEFANTE", "JIRAFA", "LEOPARDO", "COCODRILO", "HIPOPOTAMO", "RINOCERONTE", "CEBRA", "GACELA", "BUFALO", "GUEPARDO" }
                },
                new
                {
                    tema = "Países del Mundo",
                    descripcion = "Naciones de diferentes continentes",
                    palabras = new List<string> { "ARGENTINA", "COLOMBIA", "ALEMANIA", "AUSTRALIA", "JAPON", "CANADA", "BRASIL", "ITALIA", "EGIPTO", "GRECIA" }
                },
                new
                {
                    tema = "Deportes",
                    descripcion = "Actividades físicas y competitivas",
                    palabras = new List<string> { "FUTBOL", "BASQUETBOL", "NATACION", "ATLETISMO", "CICLISMO", "VOLEIBOL", "TENIS", "BEISBOL", "BOXEO", "ESCALADA" }
                },
                new
                {
                    tema = "Instrumentos Musicales",
                    descripcion = "Objetos para crear música",
                    palabras = new List<string> { "GUITARRA", "PIANO", "VIOLIN", "BATERIA", "TROMPETA", "SAXOFON", "FLAUTA", "ARPA", "CLARINETE", "TAMBOR" }
                }
            };

            var random = new Random();
            var temaSeleccionado = temas[random.Next(temas.Length)];

            return (temaSeleccionado.tema, temaSeleccionado.descripcion, temaSeleccionado.palabras);
        }


    }
}
