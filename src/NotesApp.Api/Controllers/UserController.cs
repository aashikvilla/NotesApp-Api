using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NotesApp.Application.Dto;
using NotesApp.Application.Services.Users;

namespace NotesApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IValidator<UserRegisterDto> _userRegisterValidator;
        private readonly IValidator<UserLoginDto> _userLoginValidator;

        public UserController(IUserService userService, IValidator<UserRegisterDto> userRegisterValidator, IValidator<UserLoginDto> userLoginValidator)
        {
            _userService = userService;
            _userRegisterValidator = userRegisterValidator;
            _userLoginValidator = userLoginValidator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(UserRegisterDto request)
        {
            var validationResult = _userRegisterValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var userDto = await _userService.RegisterUserAsync(request);
            return Ok(userDto);
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(UserLoginDto userLoginDto)
        {
            var validationResult = _userLoginValidator.Validate(userLoginDto);

            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var userDto = await _userService.LoginUserAsync(userLoginDto);
            return Ok(userDto);
        }
    }
}
