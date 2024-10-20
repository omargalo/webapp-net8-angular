using AppBackend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AppBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public class LoginRequest
        {
            public string? Username { get; set; }
            public string? Password { get; set; }
        }

        public class RegisterRequest
        {
            public string? Username { get; set; }
            public string? Password { get; set; }
            public string? Role { get; set; }
        }

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var token = await _authService.Authenticate(request.Username, request.Password);
            if (token == null)
                return Unauthorized(new { message = "Username or password is incorrect" });

            return Ok(new { token });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.Register(request.Username, request.Password, request.Role);
            if (!result)
                return BadRequest(new { message = "Registration failed. Username might already exist." });

            return Ok(new { message = "User registered successfully" });
        }
    }
}