using AutoMapper;
using ElAhorcadito.Helpers;
using ElAhorcadito.Models.Entities;
using ElAhorcadito.Models.DTOs.Auth;
using ElAhorcadito.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ElAhorcadito.Services
{
    public class AuthService : IAuthService
    {
        public IRepository<Usuarios> Repository { get; }
        public IRepository<RefreshTokens> RefreshTokensRepository { get; }
        public IMapper Mapper { get; }
        public JwtHelper JwtHelper { get; }
        public ITemaService TemaService { get; }
        public IRepository<ProgresoTemas> ProgresoRepository { get; }
        public IRepository<Rachas> RachasRepository { get; }
        public IRepository<Notificaciones> NotificacionesRepository { get; }

        public AuthService(IRepository<Usuarios> repository,
            IRepository<RefreshTokens> refreshTokensRepository,
            IMapper mapper,
            JwtHelper jwtHelper,
            ITemaService temaService,
            IRepository<ProgresoTemas> progresoRepository,
            IRepository<Rachas> rachasRepository,
            IRepository<Notificaciones> notificacionesRepository)
        {
            Repository = repository;
            RefreshTokensRepository = refreshTokensRepository;
            Mapper = mapper;
            JwtHelper = jwtHelper;
            TemaService = temaService;
            ProgresoRepository = progresoRepository;
            RachasRepository = rachasRepository;
            NotificacionesRepository = notificacionesRepository;
        }

        public void RegistrarUsuario(IRegistroDTO dto)
        {
            var entidad = Mapper.Map<Usuarios>(dto);
            entidad.PasswordHash = EncriptacionHelper.GetHash(dto.Password);
            entidad.FechaRegistro = DateTime.UtcNow;//
            entidad.SonidosActivados = true;
            entidad.RecordatorioDiario = true;
            Repository.Insert(entidad);
        }

        public (string, string) IniciarSesion(ILoginDTO dto)
        {
            var hash = EncriptacionHelper.GetHash(dto.Password);
            var entidad = Repository.GetAll()
                .FirstOrDefault(x => x.Email == dto.Email && x.PasswordHash == hash);

            if (entidad == null)
            {
                return (string.Empty, string.Empty);
            }

            List<Claim> claims = [
                new Claim(ClaimTypes.NameIdentifier, entidad.Id.ToString()),
                new Claim("Id", entidad.Id.ToString()),
                new Claim(ClaimTypes.Name, entidad.NombreUsuario),
                new Claim(ClaimTypes.Email, entidad.Email)
            ];

            var token = JwtHelper.GenerateJwtToken(claims);
            var refreshToken = Guid.NewGuid().ToString();

            var refreshEntity = new RefreshTokens
            {
                IdUsuario = entidad.Id,
                Token = refreshToken,
                Expiracion = DateTime.UtcNow.AddMonths(3),
                Creado = DateTime.UtcNow,
                Usado = false
            };
            RefreshTokensRepository.Insert(refreshEntity);

            return (token, refreshToken);
        }

        public (string, string) RefreshToken(string refreshToken)
        {
            var entidad = RefreshTokensRepository.GetAll()
                .Where(x => x.Token == refreshToken)
                .FirstOrDefault();

            if (entidad != null && entidad.Usado == false && entidad.Expiracion > DateTime.UtcNow)
            {
                entidad.Usado = true;
                RefreshTokensRepository.Update(entidad);

                var usuario = Repository.Get(entidad.IdUsuario);

                if (usuario == null)
                {
                    return (string.Empty, string.Empty);
                }

                List<Claim> claims = [
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim("Id", usuario.Id.ToString()),
                    new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                    new Claim(ClaimTypes.Email, usuario.Email)
                        ];

                var newToken = JwtHelper.GenerateJwtToken(claims);
                var newRefreshToken = Guid.NewGuid().ToString();

                var newRefreshEntity = new RefreshTokens
                {
                    IdUsuario = usuario.Id,
                    Token = newRefreshToken,
                    Expiracion = DateTime.UtcNow.AddMonths(3),
                    Creado = DateTime.UtcNow,
                    Usado = false
                };
                RefreshTokensRepository.Insert(newRefreshEntity);

                return (newToken, newRefreshToken);
            }

            return (string.Empty, string.Empty);
        }

        public IPerfilDTO? GetPerfil(int idUsuario)
        {
            var usuario = Repository.Get(idUsuario);
            if (usuario == null) return null;
            return Mapper.Map<PerfilDTO>(usuario); //destino - origen
            //si no coinciden las propiedades AutoMapper lo inicializa por el valor por defecto
            //para bool es false el valor por defecto.
        }

        public void ActualizarPreferencias(int idUsuario, PreferenciasDTO dto)
        {
            var usuario = Repository.Get(idUsuario);
            if (usuario != null)
            {
                usuario.SonidosActivados = dto.SonidosActivados;
                usuario.RecordatorioDiario = dto.RecordatorioDiario;
                Repository.Update(usuario);
            }
        }

        public void ReiniciarProgreso(int idUsuario)
        {
            // Obtener el usuario
            var usuario = Repository.Get(idUsuario);
            if (usuario == null)
                throw new Exception("Usuario no encontrado");

            // 1. Eliminar todo el progreso de temas
            var progresos = ProgresoRepository.GetAll()
                .Where(p => p.IdUsuario == idUsuario)
                .ToList();

            foreach (var progreso in progresos)
            {
                ProgresoRepository.Delete(progreso.Id);
            }

            // 2. Eliminar/Reiniciar rachas
            var racha = RachasRepository.GetAll()
                .FirstOrDefault(r => r.IdUsuario == idUsuario);

            if (racha != null)
            {
                RachasRepository.Delete(racha.Id);
            }

            // 3. Eliminar notificaciones
            var notificaciones = NotificacionesRepository.GetAll()
                .Where(n => n.IdUsuario == idUsuario)
                .ToList();

            foreach (var notif in notificaciones)
            {
                NotificacionesRepository.Delete(notif.Id);
            }

            // 4. Restablecer preferencias a valores por defecto
            usuario.SonidosActivados = true;
            usuario.RecordatorioDiario = true;
            Repository.Update(usuario);
        }


    }
}
