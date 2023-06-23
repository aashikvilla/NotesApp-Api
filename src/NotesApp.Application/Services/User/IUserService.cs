using NotesApp.Application.Dto;
using NotesApp.Application.Response;

namespace NotesApp.Application.Services.User
{
    public interface IUserService
    {
        Task<ServiceResponse<UserDto>> RegisterUserAsync(UserRegisterDto request);
    }

}
