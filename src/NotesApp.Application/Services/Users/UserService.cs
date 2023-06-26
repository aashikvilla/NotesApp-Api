using NotesApp.Application.Common;
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
        private readonly ITokenService _tokenService;

        public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
        }

        public async Task<ServiceResponse<UserDto>> RegisterUserAsync(UserRegisterDto userRegisterDto)
        {
            var serviceResponse = new ServiceResponse<UserDto>();
 
            if (await UserExistsAsync(userRegisterDto.Email))
            {
                serviceResponse.Message = ResponseMessages.EmailAlreadyExists;              
            }
            else
            {
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

                serviceResponse=new ServiceResponse<UserDto>
                {
                    Data = userDto,
                    Success = true,
                    Message = ResponseMessages.RegistrationSuccessful
                };
            }
            return serviceResponse;

           
        }

        private async Task<bool> UserExistsAsync(string email)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            return user != null;
        }

        public async Task<ServiceResponse<UserDto>> LoginUserAsync(UserLoginDto userLoginDto)
        {
            var serviceResponse = new ServiceResponse<UserDto>();

            var user = await _userRepository.GetUserByEmailAsync(userLoginDto.Email);

            if (user == null)
            {
                serviceResponse.Message = ResponseMessages.EmailNotFound;
            }
            else if (!_passwordHasher.VerifyPassword(user.Password, userLoginDto.Password))
            {
                serviceResponse.Message =ResponseMessages.InvalidPassword;
            }
            else
            {
                var token = _tokenService.GenerateToken(user);

                UserDto userDto = new UserDto
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Token = token
                };

                serviceResponse.Data = userDto;
                serviceResponse.Success = true;
                serviceResponse.Message = ResponseMessages.LoginSuccessful;
            }

            return serviceResponse;
        }
    }

}
