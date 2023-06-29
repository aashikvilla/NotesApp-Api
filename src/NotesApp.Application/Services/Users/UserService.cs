using NotesApp.Application.Common;
using NotesApp.Application.Dto;
using NotesApp.Domain.Entities;
using NotesApp.Domain.RepositoryInterfaces;
using NotesApp.Infrastructure.Services;

namespace NotesApp.Application.Services.Users
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;

        public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }

        public async Task<UserDto> RegisterUserAsync(UserRegisterDto userRegisterDto)
        {
            if (await UserExistsAsync(userRegisterDto.Email))
            {
                throw new InvalidOperationException(ResponseMessages.EmailAlreadyExists);
            }

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
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };

            return userDto;
        }

        private async Task<bool> UserExistsAsync(string email)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            return user != null;
        }

        public async Task<UserDto> LoginUserAsync(UserLoginDto userLoginDto)
        {
            var user = await _userRepository.GetUserByEmailAsync(userLoginDto.Email);

            if (user == null)
            {
                throw new UnauthorizedAccessException(ResponseMessages.EmailNotFound);                
            }

            if (!_passwordHasher.VerifyPassword(user.Password, userLoginDto.Password))
            {
                throw new UnauthorizedAccessException(ResponseMessages.InvalidPassword);
            }

            var token = _tokenService.GenerateToken(user);

            UserDto userDto = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Token = token
            };

            return userDto;
        }
    }
}
