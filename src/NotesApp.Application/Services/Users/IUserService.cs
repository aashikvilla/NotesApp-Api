using NotesApp.Application.Dto;
using NotesApp.Application.Response;

namespace NotesApp.Application.Services.Users
{
    public interface IUserService
    {
        Task<ServiceResponse<UserDto>> RegisterUserAsync(UserRegisterDto request);
        Task<ServiceResponse<UserDto>> LoginUserAsync(UserLoginDto userLoginDto);
    }

}
