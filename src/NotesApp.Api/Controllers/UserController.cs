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
            var result = await _userService.RegisterUserAsync(request);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }


        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(UserLoginDto userLoginDto)
        {
            var serviceResponse = await _userService.LoginUserAsync(userLoginDto);

            if (!serviceResponse.Success)
            {
                return BadRequest(serviceResponse);
            }

            return Ok(serviceResponse);
        }
    }

}
