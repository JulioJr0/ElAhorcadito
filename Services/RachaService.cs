using ElAhorcadito.Models.Entities;
using ElAhorcadito.Models.DTOs.Estadisticas;
using ElAhorcadito.Repositories;

namespace ElAhorcadito.Services
{
    public class RachaService : IRachaService
    {
        public IRepository<Rachas> RachaRepository { get; }
        public IRepository<ProgresoTemas> ProgresoRepository { get; }
        public IRepository<Notificaciones> NotificacionRepository { get; }

        public RachaService(IRepository<Rachas> rachaRepository,
            IRepository<ProgresoTemas> progresoRepository,
            IRepository<Notificaciones> notificacionRepository)
        {
            RachaRepository = rachaRepository;
            ProgresoRepository = progresoRepository;
            NotificacionRepository = notificacionRepository;
        }

        public void CheckAndExtendRacha(int idUsuario, int idTemaDiario)
        {
            var racha = RachaRepository.GetAll().FirstOrDefault(x => x.IdUsuario == idUsuario);

            if (racha == null)
            {
                racha = new Rachas
                {
                    IdUsuario = idUsuario,
                    DiasConsecutivos = 1,
                    FechaUltimaRacha = DateTime.Today,
                    PalabrasTotales = 10
                };
                RachaRepository.Insert(racha);
                GenerarNotificacionRacha(idUsuario, 1);
                return;
            }

            var ayer = DateTime.Today.AddDays(-1);

            if (racha.FechaUltimaRacha?.Date == ayer)
            {
                racha.DiasConsecutivos++;
                racha.FechaUltimaRacha = DateTime.Today;
                racha.PalabrasTotales += 10;
                RachaRepository.Update(racha);
                GenerarNotificacionRacha(idUsuario, (int)racha.DiasConsecutivos);
            }
            else if (racha.FechaUltimaRacha?.Date < ayer)
            {
                racha.DiasConsecutivos = 1;
                racha.FechaUltimaRacha = DateTime.Today;
                racha.PalabrasTotales += 10;
                RachaRepository.Update(racha);
            }
        }

        private void GenerarNotificacionRacha(int idUsuario, int diasConsecutivos)
        {
            var mensaje = diasConsecutivos switch
            {
                1 => "¡Comenzaste tu racha! Completa el reto mañana para seguir.",
                7 => "¡Increíble! 1 semana de racha consecutiva. ¡Sigue así!",
                30 => "🔥 ¡1 MES DE RACHA! Eres imparable.",
                _ => $"¡Racha de {diasConsecutivos} días! Mantén tu constancia."
            };

            var notificacion = new Notificaciones
            {
                IdUsuario = idUsuario,
                TipoNotificacion = "RACHA",
                Titulo = $"¡Racha de {diasConsecutivos} días!",
                Mensaje = mensaje,
                FechaCreacion = DateTime.UtcNow,
                Leida = false
            };
            NotificacionRepository.Insert(notificacion);
        }

        public EstadisticasDTO GetEstadisticas(int idUsuario)
        {
            var racha = RachaRepository.GetAll().FirstOrDefault(x => x.IdUsuario == idUsuario);
            var temasCompletados = ProgresoRepository.GetAll()
                .Count(x => x.IdUsuario == idUsuario && x.PalabrasCompletadas >= 10);

            var progresoSemanal = CalcularProgresoSemanal(idUsuario);

            return new EstadisticasDTO
            {
                DiasConsecutivos = racha?.DiasConsecutivos ?? 0,
                PalabrasTotales = racha?.PalabrasTotales ?? 0,
                TemasCompletados = temasCompletados,
                ProgresoSemanal = progresoSemanal
            };
        }

        private Dictionary<string, bool> CalcularProgresoSemanal(int idUsuario)
        {
            var hoy = DateTime.Today;
            var inicioSemana = hoy.AddDays(-(int)hoy.DayOfWeek);

            var progresosCompletados = ProgresoRepository.GetAll()
            .Where(x => x.IdUsuario == idUsuario
                     && x.FechaUltimaActividad >= inicioSemana
                     && x.PalabrasCompletadas >= 10) //Solo temas completados
            .ToList();

            var diasSemana = new[] { "Dom", "Lun", "Mar", "Mie", "Jue", "Vie", "Sab" };
            var resultado = new Dictionary<string, bool>();

            for (int i = 0; i < 7; i++)
            {
                var fecha = inicioSemana.AddDays(i);
                var completoRetoDiario = progresosCompletados.Any(p => p.FechaUltimaActividad.Date == fecha);
                resultado[diasSemana[i]] = completoRetoDiario;
            }

            return resultado;
        }





    }
}
