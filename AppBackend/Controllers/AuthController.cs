using AppBackend.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace AppBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        // Login request model
        public class LoginRequest
        {
            [Required(ErrorMessage = "Username is required")]
            public string? Username { get; set; }

            [Required(ErrorMessage = "Password is required")]
            public string? Password { get; set; }
        }

        // Register request model
        public class RegisterRequest
        {
            [Required(ErrorMessage = "Username is required")]
            public string? Username { get; set; }

            [Required(ErrorMessage = "Password is required")]
            public string? Password { get; set; }

            [Required(ErrorMessage = "Role is required")]
            public string? Role { get; set; }

            // New fields
            [Required(ErrorMessage = "Name is required")]
            public string? Name { get; set; }

            [Required(ErrorMessage = "Last name is required")]
            public string? LastName { get; set; }

            [Required(ErrorMessage = "Mother's maiden name is required")]
            public string? MothersMaidenName { get; set; }

            [Required(ErrorMessage = "Email is required")]
            [EmailAddress]
            public string? Email { get; set; }

            [Required(ErrorMessage = "Cell phone is required")]
            [Phone]
            public string? CellPhone { get; set; }
        }

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // Login endpoint
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Explicit null checks to satisfy the compiler
            if (request.Username is null || request.Password is null)
            {
                return BadRequest(new { message = "Username and password are required." });
            }

            var token = await _authService.Authenticate(request.Username, request.Password);
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { message = "Username or password is incorrect." });
            }

            return Ok(new { token });
        }


        // Register endpoint
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Explicit null checks for required fields
            if (request.Username is null || request.Password is null || request.Role is null ||
                request.Name is null || request.LastName is null || request.MothersMaidenName is null ||
                request.Email is null || request.CellPhone is null)
            {
                return BadRequest(new { message = "All fields are required." });
            }

            // Pass all the fields to the AuthService's Register method
            var result = await _authService.Register(
                request.Username,
                request.Password,
                request.Role,
                request.Name,
                request.LastName,
                request.MothersMaidenName,
                request.Email,
                request.CellPhone);

            if (!result)
            {
                return BadRequest(new { message = "Registration failed. Username might already exist." });
            }

            return Ok(new { message = "User registered successfully" });
        }
    }
}
