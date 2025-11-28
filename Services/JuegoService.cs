using ElAhorcadito.Models.Entities;
using ElAhorcadito.Models.DTOs.Juego;
using ElAhorcadito.Repositories;
using System.Linq;

namespace ElAhorcadito.Services
{
    public class JuegoService : IJuegoService
    {
        public ITemaService TemaService { get; }
        public IRepository<ProgresoTemas> ProgresoRepository { get; }
        public IRepository<Palabras> PalabraRepository { get; }
        public IRachaService RachaService { get; }

        public JuegoService(ITemaService temaService,
            IRepository<ProgresoTemas> progresoRepository,
            IRepository<Palabras> palabraRepository,
            IRachaService rachaService)
        {
            TemaService = temaService;
            ProgresoRepository = progresoRepository;
            PalabraRepository = palabraRepository;
            RachaService = rachaService;
        }

        public async Task<RetoDiarioDTO> IniciarRetoDiario(int idUsuario)
        {
            var temaDiario = await TemaService.GetOrCreateTemaDiario();

            var progreso = ProgresoRepository.GetAll()
                .FirstOrDefault(x => x.IdUsuario == idUsuario && x.IdTema == temaDiario.Id);

            if (progreso == null)
            {
                progreso = new ProgresoTemas
                {
                    IdUsuario = idUsuario,
                    IdTema = temaDiario.Id,
                    PalabrasCompletadas = 0,
                    FechaUltimaActividad = DateTime.UtcNow
                };
                ProgresoRepository.Insert(progreso);
            }

            var palabraActualIndex = progreso.PalabrasCompletadas ?? 0;

            var palabrasDelTema = PalabraRepository.GetAll()
                .Where(x => x.IdTema == temaDiario.Id)
                .OrderBy(x => x.Id)
                .ToList();

            var palabraAdivinar = palabrasDelTema
                .Skip((int)palabraActualIndex)
                .FirstOrDefault();

            var fechaLimite = DateTime.Today.AddDays(1).AddSeconds(-1);

            return new RetoDiarioDTO
            {
                IdTema = temaDiario.Id,
                NombreTema = temaDiario.Nombre ?? "",
                PalabraActualIndex = (int)palabraActualIndex,
                PalabrasCompletadas = (int)palabraActualIndex,
                TotalPalabras = 10,
                PalabraAdivinar = palabraAdivinar?.Palabra ?? "",
                FechaLimite = fechaLimite,
                YaCompletadoHoy = palabraActualIndex >= 10
            };
        }

        public PalabraActualDTO? ObtenerPalabraActual(int idUsuario, int idTema)
        {
            var progreso = ProgresoRepository.GetAll()
                .FirstOrDefault(x => x.IdUsuario == idUsuario && x.IdTema == idTema);

            var palabraIndex = progreso?.PalabrasCompletadas ?? 0;

            if (palabraIndex >= 10)
            {
                return null;
            }

            var palabra = PalabraRepository.GetAll()
                .Where(x => x.IdTema == idTema)
                .OrderBy(x => x.Id)
                .Skip(palabraIndex)
                .FirstOrDefault();

            if (palabra == null) return null;

            return new PalabraActualDTO
            {
                PalabraIndex = palabraIndex,
                Palabra = palabra.Palabra,
                PalabrasCompletadas = palabraIndex,
                TotalPalabras = 10
            };
        }

        public void ActualizarProgresoPalabra(int idUsuario, ActualizarProgresoDTO dto)
        {
            var progreso = ProgresoRepository.GetAll()
                .FirstOrDefault(x => x.IdUsuario == idUsuario && x.IdTema == dto.IdTema);

            if (progreso == null) return;

            if (dto.Resultado == "GANADA")
            {
                progreso.PalabrasCompletadas++;
                progreso.FechaUltimaActividad = DateTime.UtcNow;
                ProgresoRepository.Update(progreso);

                if (progreso.PalabrasCompletadas >= 10)
                {
                    RachaService.CheckAndExtendRacha(idUsuario, dto.IdTema);
                }
            }
        }

        public async Task SincronizarProgresoOffline(int idUsuario, ProgresoOfflineDTO dto)
        {
            var temaDiario = await TemaService.GetOrCreateTemaDiario();

            foreach (var partida in dto.Partidas)
            {
                var progreso = ProgresoRepository.GetAll()
                    .FirstOrDefault(x => x.IdUsuario == idUsuario && x.IdTema == partida.IdTema);

                if (progreso == null)
                {
                    progreso = new ProgresoTemas
                    {
                        IdUsuario = idUsuario,
                        IdTema = partida.IdTema,
                        PalabrasCompletadas = 0,
                        FechaUltimaActividad = DateTime.UtcNow
                    };
                    ProgresoRepository.Insert(progreso);
                }

                if (partida.Ganada)
                {
                    progreso.PalabrasCompletadas++;
                    progreso.FechaUltimaActividad = partida.FechaJuego;
                    ProgresoRepository.Update(progreso);
                }
            }

            var progresoDiario = ProgresoRepository.GetAll()
                .FirstOrDefault(x => x.IdUsuario == idUsuario && x.IdTema == temaDiario.Id);

            if (progresoDiario != null && progresoDiario.PalabrasCompletadas >= 10)
            {
                RachaService.CheckAndExtendRacha(idUsuario, temaDiario.Id);
            }
        }








    }
}
