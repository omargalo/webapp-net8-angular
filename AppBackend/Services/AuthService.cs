using AppBackend.Data;
using AppBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BCryptNet = BCrypt.Net.BCrypt;



namespace AppBackend.Services
{
    public interface IAuthService
    {
        Task<string> Authenticate(string username, string password);
        Task<bool> Register(string username, string password, string role);
    }

    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _jwtSecret;
        private ApplicationDbContext context;
        private string jwtSecretKey;

        public AuthService(ApplicationDbContext context, string jwtSecret)
        {
            _context = context;
            _jwtSecret = jwtSecret ?? throw new ArgumentNullException(nameof(jwtSecret));
        }

        public async Task<string> Authenticate(string username, string password)
        {
            // Validate the user
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role) // Include related roles
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null || !VerifyPassword(password, user.PasswordHash))
                return null;

            // Extract the role (for simplicity assuming one role per user)
            var userRole = user.UserRoles.FirstOrDefault()?.Role?.Name;

            if (userRole == null)
                return null;

            // Generate the JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, userRole)
        }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        public async Task<bool> Register(string username, string password, string roleName)
        {
            // Check if the user already exists
            if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                return false;
            }

            // Hash the password
            var hashedPassword = BCryptNet.HashPassword(password);

            // Create the new user
            var user = new User
            {
                Username = username,
                PasswordHash = hashedPassword,
                Status = true // Active user
            };

            // Find the role by its name
            var role = await _context.CatRoles.FirstOrDefaultAsync(r => r.Name == roleName);

            if (role == null)
            {
                return false; // Invalid role
            }

            // Create the UserRole relationship
            var userRole = new UserRole
            {
                User = user,
                Role = role,
                Status = true // Active role assignment
            };

            // Save the new user and the UserRole assignment
            _context.Users.Add(user);
            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();

            return true;
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            // Verificar la contraseña utilizando BCrypt
            return BCryptNet.Verify(password, storedHash);
        }
    }
}
