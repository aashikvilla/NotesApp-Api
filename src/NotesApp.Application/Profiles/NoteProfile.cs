using AutoMapper;
using NotesApp.Application.Dto;
using NotesApp.Domain.Entities;

namespace NotesApp.Application.Profiles
{
    public class NoteProfile : Profile
    {
        public NoteProfile()
        {
            CreateMap<Note, NoteDto>().ReverseMap();
        }
    }

}
