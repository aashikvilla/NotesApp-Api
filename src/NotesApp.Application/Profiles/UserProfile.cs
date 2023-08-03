using AutoMapper;
using NotesApp.Application.Dto;
using NotesApp.Domain.Entities;

namespace NotesApp.Application.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserRegisterDto, User>();
            CreateMap<User, UserDto>();
        }
    }
}
