using AppBackend.Data;
using Microsoft.EntityFrameworkCore;
using DotNetEnv;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

var builder = WebApplication.CreateBuilder(args);

// Cargar las variables de entorno
Env.Load();

// Acceder a las variables de entorno
string? dbHost = Environment.GetEnvironmentVariable("AZOGASQL_HOST");
string? dbName = Environment.GetEnvironmentVariable("AZOGASQL_DB");
string? dbUser = Environment.GetEnvironmentVariable("AZOGASQL_USER");
string? dbPassword = Environment.GetEnvironmentVariable("AZOGASQL_PASSWORD");
string? jwtSecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");

// Construir la cadena de conexión
string connectionString = $"Server=tcp:{dbHost},1433;Initial Catalog={dbName};Persist Security Info=False;User ID={dbUser};Password={dbPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

// Registrar el contexto de la base de datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure();
    }));

// Add services to the container.

builder.Services.AddControllers();
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

app.UseAuthorization();

app.MapControllers();

app.Run();
