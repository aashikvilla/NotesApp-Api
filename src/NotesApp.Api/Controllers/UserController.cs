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

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(UserRegisterDto request)
        {
            var userDto = await _userService.RegisterUserAsync(request);
            return Ok(userDto);
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(UserLoginDto userLoginDto)
        {
            var userDto = await _userService.LoginUserAsync(userLoginDto);
            return Ok(userDto);
        }
    }

}
