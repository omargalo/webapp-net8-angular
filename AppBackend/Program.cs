using AppBackend.Data;
using AppBackend.Interfaces;
using AppBackend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Diagnostics;
using System.Text;

// Create the builder
var builder = WebApplication.CreateBuilder(args);

// Configure appsettings.json and environment vthis ariables
builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Access configuration variables using the Configuration API
string dbHost = builder.Configuration["GAHT_SQL_HOST"] ?? throw new InvalidOperationException("GAHT_SQL_HOST is not configured");
string dbName = builder.Configuration["GAHT_SQL_DB"] ?? throw new InvalidOperationException("GAHT_SQL_DB is not configured");
string dbUser = builder.Configuration["GAHT_SQL_USER"] ?? throw new InvalidOperationException("GAHT_SQL_USER is not configured");
string dbPassword = builder.Configuration["GAHT_SQL_PASSWORD"] ?? throw new InvalidOperationException("GAHT_SQL_PASSWORD is not configured");
string jwtSecretKey = builder.Configuration["GAHT_JWT_SECRET_KEY"] ?? throw new InvalidOperationException("GAHT_JWT_SECRET_KEY is not configured");

// Build the connection string
string connectionString = $"Server=tcp:{dbHost},1433;Initial Catalog={dbName};Persist Security Info=False;User ID={dbUser};Password={dbPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

// Register the database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure();
    }));

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

// Configure Authorization with roles
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("User", policy => policy.RequireRole("User"));
});

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddScoped<IAuthService, AuthService>(provider =>
{
    var context = provider.GetRequiredService<ApplicationDbContext>();
    return new AuthService(context, jwtSecretKey);
});

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "GAHT API", Version = "v1" });

    // Add JWT Authentication scheme
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by your token in the text input below.\nExample: 'Bearer 12345abcdef'",
    });

    // Add a global security requirement
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Build the app
var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GAHT API V1");
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
