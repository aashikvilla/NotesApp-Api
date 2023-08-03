using AutoMapper;
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
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenService tokenService, IMapper mapper)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _mapper = mapper;
        }

        public async Task<UserDto> RegisterUserAsync(UserRegisterDto userRegisterDto)
        {
            if (await UserExistsAsync(userRegisterDto.Email))
            {
                throw new InvalidOperationException(ResponseMessages.EmailAlreadyExists);
            }

            User user = _mapper.Map<User>(userRegisterDto);
            user.Password = _passwordHasher.HashPassword(userRegisterDto.Password);

            await _userRepository.AddUserAsync(user);

            UserDto userDto = _mapper.Map<UserDto>(user);

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
            UserDto userDto = _mapper.Map<UserDto>(user);
            userDto.Token = token;
            return userDto;
        }
    }
}
