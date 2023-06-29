using NotesApp.Application.Dto;

namespace NotesApp.Application.Services.Users
{
    public interface IUserService
    {
        Task<UserDto> RegisterUserAsync(UserRegisterDto request);
        Task<UserDto> LoginUserAsync(UserLoginDto userLoginDto);
    }

}
