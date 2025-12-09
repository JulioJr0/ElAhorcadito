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

        public AuthService(IRepository<Usuarios> repository,
            IRepository<RefreshTokens> refreshTokensRepository,
            IMapper mapper,
            JwtHelper jwtHelper,
            ITemaService temaService)
        {
            Repository = repository;
            RefreshTokensRepository = refreshTokensRepository;
            Mapper = mapper;
            JwtHelper = jwtHelper;
            TemaService = temaService;
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

        //public (string, string) RefreshToken(string refreshToken)
        //{
        //    var storedToken = RefreshTokensRepository.GetAll()
        //        .FirstOrDefault(x => x.Token == refreshToken &&
        //        !(x.Usado ?? false) &&
        //        x.Expiracion > DateTime.UtcNow);

        //    if (storedToken == null)
        //    {
        //        return (string.Empty, string.Empty);
        //    }

        //    storedToken.Usado = true;
        //    RefreshTokensRepository.Update(storedToken);

        //    var usuario = Repository.Get(storedToken.IdUsuario);
        //    if (usuario == null)
        //    {
        //        return (string.Empty, string.Empty);
        //    }

        //    List<Claim> claims = [
        //        new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
        //        new Claim("Id", usuario.Id.ToString()),
        //        new Claim(ClaimTypes.Name, usuario.NombreUsuario),
        //        new Claim(ClaimTypes.Email, usuario.Email)
        //    ];

        //    var newToken = JwtHelper.GenerateJwtToken(claims);
        //    var newRefreshToken = Guid.NewGuid().ToString();

        //    var newRefreshEntity = new RefreshTokens
        //    {
        //        IdUsuario = usuario.Id,
        //        Token = newRefreshToken,
        //        Expiracion = DateTime.UtcNow.AddMonths(3),
        //        Creado = DateTime.UtcNow,
        //        Usado = false
        //    };
        //    RefreshTokensRepository.Insert(newRefreshEntity);

        //    return (newToken, newRefreshToken);
        //}

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





    }
}
