using AutoMapper;
using digitalAlbumApi.DTOs.UserDtos;
using digitalAlbumApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace digitalAlbumApi.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<CreateUserDto, User >();
        }
    }
}
