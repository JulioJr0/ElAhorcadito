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

        public async Task<Temas> GetOrCreateTemaDiario()
        {
            var hoy = DateTime.Today;
            var temaExistente = TemasRepository.GetAll()
                .FirstOrDefault(x => x.FechaGeneracion.Date == hoy);

            if (temaExistente != null)
            {
                return temaExistente;
            }

            var (nombreTema, descripcion, palabras) = await GeminiService.GenerarTemaYPalabras();

            var nuevoTema = new Temas
            {
                Nombre = nombreTema,
                Descripcion = descripcion,
                PromptBase = $"Tema generado con IA: {nombreTema}",
                FechaGeneracion = hoy,
                GeneradoPorIa = true
            };
            TemasRepository.Insert(nuevoTema);

            foreach (var palabra in palabras)
            {
                PalabrasRepository.Insert(new Palabras
                {
                    IdTema = nuevoTema.Id,
                    Palabra = palabra
                });
            }

            return nuevoTema;
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

                //cifrar las palabras (Base64 simple)
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
