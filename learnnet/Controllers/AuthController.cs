using learnnet.DTOs;
using learnnet.Services;
using Microsoft.AspNetCore.Mvc;

namespace learnnet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto request)
        {
            var response = await _authService.Register(request);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto request)
        {
            var response = await _authService.Login(request);
            if (!response.Success)
            {
                return Unauthorized(response);
            }
            return Ok(response);
        }
    }
}
