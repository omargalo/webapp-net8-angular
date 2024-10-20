using AppBackend.Data;
using AppBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;


namespace AppBackend.Services
{
    public interface IAuthService
    {
        Task<string> Authenticate(string username, string password);
        Task<bool> Register(string username, string password, string role); // Añadido método Register
    }

    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _jwtSecret;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _jwtSecret = configuration["JWT_SECRET_KEY"];
        }

        public async Task<string> Authenticate(string username, string password)
        {
            // Validar el usuario
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null || !VerifyPassword(password, user.PasswordHash))
                return null;

            // Generar el token JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<bool> Register(string username, string password, string role)
        {
            // Verificar si el usuario ya existe
            if (await _context.Users.AnyAsync(u => u.Username == username))
            {
                // El usuario ya existe
                return false;
            }

            // Hashear la contraseña
            //var hashedPassword = BCrypt.HashPassword(password);
            var hashedPassword = BCrypt.HashPassword(password);

            // Crear el nuevo usuario
            var user = new User
            {
                Username = username,
                PasswordHash = hashedPassword,
                Role = role
            };

            // Agregar y guardar el usuario en la base de datos
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            // Verificar la contraseña utilizando BCrypt
            return BCrypt.Verify(password, storedHash);
        }
    }
}
