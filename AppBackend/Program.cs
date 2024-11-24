using AppBackend.Data;
using AppBackend.Interfaces;
using AppBackend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using System;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add enhanced logging providers
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Build logger explicitly if needed
var loggerFactory = LoggerFactory.Create(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});
var logger = loggerFactory.CreateLogger<Program>();
logger.LogInformation("Application starting...");

// Configuration validation with logging
string GetRequiredConfig(string key)
{
    string? value = builder.Configuration[key];
    if (value is null)
    {
        logger.LogCritical("{Key} is not configured. Application cannot start.", key);
        throw new InvalidOperationException($"{key} is not configured");
    }
    logger.LogInformation("{Key} successfully loaded.", key);
    return value;
}

// Configuration
string dbHost = GetRequiredConfig("GAHT_SQL_HOST");
string dbName = GetRequiredConfig("GAHT_SQL_DB");
string dbUser = GetRequiredConfig("GAHT_SQL_USER");
string dbPassword = GetRequiredConfig("GAHT_SQL_PASSWORD");
string jwtSecretKey = GetRequiredConfig("GAHT_JWT_SECRET_KEY");

// Connection string
string connectionString = $"Server=tcp:{dbHost},1433;Initial Catalog={dbName};Persist Security Info=False;User ID={dbUser};Password={dbPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;";
logger.LogInformation("Database connection string created.");

// Add services
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure();
    }));

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
logger.LogInformation("JWT authentication configured.");

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("User", policy => policy.RequireRole("User"));
});
logger.LogInformation("Authorization policies added.");

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("https://example.com")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
logger.LogInformation("CORS policy configured.");

builder.Services.AddControllers();
builder.Services.AddScoped<IAuthService, AuthService>(provider =>
{
    var context = provider.GetRequiredService<ApplicationDbContext>();
    return new AuthService(context, jwtSecretKey);
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "GAHT API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by your token in the text input below.\nExample: 'Bearer 12345abcdef'"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
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
logger.LogInformation("Swagger/OpenAPI configured.");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GAHT API V1");
    });
    logger.LogInformation("Swagger UI enabled for development.");
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
    logger.LogInformation("Exception handler and HSTS enabled for production.");
}

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigins");
logger.LogInformation("CORS policy applied.");

app.UseAuthentication();
logger.LogInformation("Authentication middleware enabled.");

app.UseAuthorization();
logger.LogInformation("Authorization middleware enabled.");

app.MapControllers();
logger.LogInformation("Controllers mapped.");

logger.LogInformation("Application running...");
app.Run();
