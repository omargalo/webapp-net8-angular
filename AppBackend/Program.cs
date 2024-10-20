using AppBackend.Data;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System;
using AppBackend.Services;

var builder = WebApplication.CreateBuilder(args);

// Cargar las variables de entorno
Env.Load();

// Acceder a las variables de entorno
string? dbHost = Environment.GetEnvironmentVariable("AZOGASQL_HOST");
string? dbName = Environment.GetEnvironmentVariable("AZOGASQL_DB");
string? dbUser = Environment.GetEnvironmentVariable("AZOGASQL_USER");
string? dbPassword = Environment.GetEnvironmentVariable("AZOGASQL_PASSWORD");
string? jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");

// Construir la cadena de conexi�n
string connectionString = $"Server=tcp:{dbHost},1433;Initial Catalog={dbName};Persist Security Info=False;User ID={dbUser};Password={dbPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

// Registrar el contexto de la base de datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure();
    }));

// Configurar autenticaci�n JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false, // Configura seg�n tus necesidades
        ValidateAudience = false, // Configura seg�n tus necesidades
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// Configurar autorizaci�n con roles
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("User", policy => policy.RequireRole("User"));
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddScoped<IAuthService, AuthService>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
