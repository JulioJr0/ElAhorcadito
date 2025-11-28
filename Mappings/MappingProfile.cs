using AutoMapper;
using ElAhorcadito.Models.Entities;
using ElAhorcadito.Models.DTOs.Auth;

namespace ElAhorcadito.Mappings
{
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            CreateMap<RegistroDTO, Usuarios>();
            CreateMap<Usuarios, PerfilDTO>();
        }
    }
}
