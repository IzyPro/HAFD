using System;
using AutoMapper;
using HAFD.Models;

namespace HAFD.DTOs
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<User, LoginResponseDTO>();
            CreateMap<User, UserProfileResponseDTO>();
        }
    }
}
