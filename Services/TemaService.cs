using ElAhorcadito.Models.Entities;
using ElAhorcadito.Repositories;

namespace ElAhorcadito.Services
{
    public class TemaService : ITemaService
    {
        public IRepository<Temas> TemasRepository { get; }
        public IRepository<Palabras> PalabrasRepository { get; }
        public IRepository<ProgresoTemas> ProgresoRepository { get; }
        public IGeminiService GeminiService { get; }

        public TemaService(IRepository<Temas> temasRepository,
            IRepository<Palabras> palabrasRepository,
            IRepository<ProgresoTemas> progresoRepository,
            IGeminiService geminiService)
        {
            TemasRepository = temasRepository;
            PalabrasRepository = palabrasRepository;
            ProgresoRepository = progresoRepository;
            GeminiService = geminiService;
        }
        public async Task CrearTemasFaltantes(DateTime? fechaInicio = null)
        {
            try
            {
                //Si no se proporciona fecha, buscar el último tema creado
                var ultimoTema = TemasRepository.GetAll()
                    .OrderByDescending(t => t.FechaGeneracion)
                    .FirstOrDefault();

                DateTime fechaDesde;

                if (ultimoTema == null)
                {
                    //Si no hay ningún tema, crear desde hece 1 día
                    fechaDesde = DateTime.Today.AddDays(-1);
                }
                else if (fechaInicio.HasValue)
                {
                    fechaDesde = fechaInicio.Value.Date;
                }
                else
                {
                    //Crear desde el día siguiente al último tema
                    fechaDesde = ultimoTema.FechaGeneracion.Date.AddDays(1);
                }

                DateTime hoy = DateTime.Today;

                //Crear temas para cada día faltante
                for (DateTime fecha = fechaDesde; fecha <= hoy; fecha = fecha.AddDays(1))
                {
                    //Verificar si ya existe tema para esta fecha
                    var temaExistente = TemasRepository.GetAll()
                        .FirstOrDefault(t => t.FechaGeneracion.Date == fecha);

                    if (temaExistente == null)
                    {
                        await CrearTemaDiario(fecha);

                        //Pequeña pausa entre creaciones para no saturar la API de Gemini
                        await Task.Delay(1000);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creando temas faltantes: {ex.Message}");
                //No lanzar excepción para no interrumpir el login
            }
        }

        //Crear tema para una fecha específica
        private async Task<Temas> CrearTemaDiario(DateTime fecha)
        {
            Temas nuevoTema;
            List<string> palabras;

            try
            {
                var (nombreTema, descripcion, palabrasGeneradas) = await GeminiService.GenerarTemaYPalabras();

                //Validar que Gemini devolvió datos válidos
                if (string.IsNullOrWhiteSpace(nombreTema) || palabrasGeneradas == null || palabrasGeneradas.Count < 10)
                {
                    Console.WriteLine($"Gemini devolvió datos incompletos para {fecha:dd/MM/yyyy}. Usando tema de respaldo.");
                    return CrearTemaRespaldo(fecha);
                }

                nuevoTema = new Temas
                {
                    Nombre = nombreTema,
                    Descripcion = descripcion ?? "Tema del día",
                    PromptBase = $"Tema generado con IA: {nombreTema}",
                    FechaGeneracion = fecha,
                    GeneradoPorIa = true
                };

                palabras = palabrasGeneradas;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error llamando a Gemini para {fecha:dd/MM/yyyy}: {ex.Message}");
                return CrearTemaRespaldo(fecha);
            }

            try
            {
                TemasRepository.Insert(nuevoTema);

                foreach (var palabra in palabras)
                {
                    PalabrasRepository.Insert(new Palabras
                    {
                        IdTema = nuevoTema.Id,
                        Palabra = palabra
                    });
                }

                Console.WriteLine($"Tema creado exitosamente: {nuevoTema.Nombre} ({fecha:dd/MM/yyyy})");
                return nuevoTema;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error insertando tema en BD para {fecha:dd/MM/yyyy}: {ex.Message}");
                throw; //Re-lanzar para que el método padre lo maneje
            }
        }

        //Crear tema de respaldo
        private Temas CrearTemaRespaldo(DateTime fecha)
        {
            try
            {
                var temaRespaldo = new Temas
                {
                    Nombre = $"Programación {fecha:dd/MM/yyyy}",
                    Descripcion = "Conceptos fundamentales de programación",
                    PromptBase = "Tema de respaldo - Programación",
                    FechaGeneracion = fecha,
                    GeneradoPorIa = false
                };
                TemasRepository.Insert(temaRespaldo);
                //Palabras de respaldo
                var palabrasRespaldo = new List<string>
                {
                    "PROGRAMACION", "JAVASCRIPT", "PYTHON", "DATABASE",
                    "SERVIDOR", "FRONTEND", "BACKEND", "ALGORITMO",
                    "VARIABLE", "FUNCION"
                };
                foreach (var palabra in palabrasRespaldo)
                {
                    PalabrasRepository.Insert(new Palabras
                    {
                        IdTema = temaRespaldo.Id,
                        Palabra = palabra
                    });
                }
                Console.WriteLine($"Tema de respaldo creado para {fecha:dd/MM/yyyy}");
                return temaRespaldo;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error crítico creando tema de respaldo: {ex.Message}");
                throw;
            }
        }

        public async Task<Temas> GetOrCreateTemaDiario()
        {
            var hoy = DateTime.Today;
            var temaExistente = TemasRepository.GetAll()
                .FirstOrDefault(x => x.FechaGeneracion.Date == hoy);

            if (temaExistente != null)
            {
                return temaExistente;
            }
            return await CrearTemaDiario(hoy);
        }

        public IEnumerable<object> ObtenerPaginaTemas(int pageNumber, int idUsuario)
        {
            int pageSize = 10;
            var temas = TemasRepository.GetAll()
                .OrderByDescending(x => x.FechaGeneracion)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);

            var resultado = new List<object>();
            foreach (var tema in temas)
            {
                var progreso = ProgresoRepository.GetAll()
                    .FirstOrDefault(x => x.IdUsuario == idUsuario && x.IdTema == tema.Id);

                var palabras = PalabrasRepository.GetAll()
                    .Where(x => x.IdTema == tema.Id)
                    .OrderBy(x => x.Id)
                    .Select(x => x.Palabra)
                    .ToList();
                //Cifrar las palabras (Base64 simple)
                var palabrasCifradas = palabras.Select(p =>
                    Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(p))
                ).ToList();
                resultado.Add(new
                {
                    IdTema = tema.Id,
                    Nombre = tema.Nombre,
                    Descripcion = tema.Descripcion ?? "",
                    FechaGeneracion = tema.FechaGeneracion,
                    PalabrasCompletadas = progreso?.PalabrasCompletadas ?? 0,
                    TotalPalabras = 10,
                    PorcentajeProgreso = ((progreso?.PalabrasCompletadas ?? 0) / 10.0) * 100,
                    PalabrasCifradas = palabrasCifradas
                });
            }
            return resultado;
        }
    }
}
