using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AppBackend.Data;
using AppBackend.Interfaces;
using AppBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using BCryptNet = BCrypt.Net.BCrypt;

namespace AppBackend.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _jwtSecret;

        public AuthService(ApplicationDbContext context, string jwtSecret)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _jwtSecret = jwtSecret ?? throw new ArgumentNullException(nameof(jwtSecret));
        }

        public async Task<string> Authenticate(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Username or password cannot be empty.");

            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == username)
                ?? throw new UnauthorizedAccessException("User not found or password invalid.");

            if (!VerifyPassword(password, user.PasswordHash))
                throw new UnauthorizedAccessException("User not found or password invalid.");

            var userRole = user.UserRoles.FirstOrDefault()?.Role?.Name
                ?? throw new InvalidOperationException("User role is not assigned.");

            return GenerateJwtToken(user.Username, userRole);
        }

        public async Task<bool> Register(
            string username, string password, string roleName,
            string name, string lastName, string mothersMaidenName,
            string email, string cellPhone)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(roleName) || string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Required fields cannot be empty.");
            }

            if (await _context.Users.AnyAsync(u => u.Username == username))
                throw new InvalidOperationException("A user with this username already exists.");

            var role = await _context.CatRoles
                .FirstOrDefaultAsync(r => r.Name == roleName)
                ?? throw new InvalidOperationException("Invalid role.");

            var user = new User
            {
                Username = username,
                PasswordHash = BCryptNet.HashPassword(password),
                Name = name,
                LastName = lastName,
                MothersMaidenName = mothersMaidenName,
                Email = email,
                CellPhone = cellPhone,
                Status = true
            };

            var userRole = new UserRole
            {
                User = user,
                Role = role,
                Status = true
            };

            await _context.Users.AddAsync(user);
            await _context.UserRoles.AddAsync(userRole);
            await _context.SaveChangesAsync();

            return true;
        }

        private string GenerateJwtToken(string username, string role)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSecret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, role)
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private static bool VerifyPassword(string password, string storedHash)
        {
            return BCryptNet.Verify(password, storedHash);
        }
    }
}
