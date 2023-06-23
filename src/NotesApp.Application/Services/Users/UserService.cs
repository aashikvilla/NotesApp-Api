using NotesApp.Application.Dto;
using NotesApp.Application.Response;
using NotesApp.Domain.Entities;
using NotesApp.Domain.RepositoryInterfaces;
using NotesApp.Infrastructure.Services;

namespace NotesApp.Application.Services.Users
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<ServiceResponse<UserDto>> RegisterUserAsync(UserRegisterDto userRegisterDto)
        {
            // Check if the user already exists
            if (await UserExistsAsync(userRegisterDto.Email))
            {
                return new ServiceResponse<UserDto>
                {
                    Success = false,
                    Message = "Email already exists."
                };
            }

            // Create the user

            User user = new User
            {
                FirstName = userRegisterDto.FirstName,
                LastName = userRegisterDto.LastName,
                Email = userRegisterDto.Email,
                Password = _passwordHasher.HashPassword(userRegisterDto.Password)
            };

            await _userRepository.AddUserAsync(user);

            UserDto userDto = new UserDto
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };

            return new ServiceResponse<UserDto>
            {
                Data = userDto,
                Success = true,
                Message = "User registered successfully."
            };
        }

        private async Task<bool> UserExistsAsync(string email)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            return user != null;
        }
    }

}
