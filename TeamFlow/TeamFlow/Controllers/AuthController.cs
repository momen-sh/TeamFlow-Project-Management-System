using Microsoft.AspNetCore.Mvc;
using TeamFlow.DTOs.Auth;
using TeamFlow.DTOs.Common;
using TeamFlow.Services.Interfaces;

namespace TeamFlow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var token = await _authService.RegisterAsync(dto);
            if (token is null)
                return BadRequest(ApiResponse<object>.Fail("User already exists"));

            return Ok(ApiResponse<object>.Ok(new { token }, "Registration successful"));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var token = await _authService.LoginAsync(dto);
            if (token is null)
                return Unauthorized(ApiResponse<object>.Fail("Invalid credentials"));

            return Ok(ApiResponse<object>.Ok(new { token }, "Login successful"));
        }
    }
}
