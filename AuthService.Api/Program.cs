using AuthService.Application.Common.Behaviors;
using AuthService.Domain.Constants;
using AuthService.Infrastructure;
using AuthService.Infrastructure.Persistence;
using FluentValidation;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.IO.Compression;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .WriteTo.Console());

builder.Services.AddControllers();

// ============================================
// OPTIMIZATION: Response Compression
// ============================================
// Static readonly array for MIME types (fixes CA1861)
var additionalMimeTypes = new[]
{
    "application/json",
    "application/javascript",
    "text/json",
    "text/javascript"
};

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(additionalMimeTypes);
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

// ============================================
// OPTIMIZATION: Response Caching
// ============================================
builder.Services.AddResponseCaching();

// CORS Configuration - Allow Blazor WebAssembly client
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins(
    "https://localhost:22500",  // Blazor WebAssembly
    "https://localhost:25650",  // Gateway
    "http://localhost:22400",   // HTTP fallback
    "http://localhost:25600"    // HTTP fallback
)
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()
        .WithExposedHeaders("X-Pagination"); // For pagination headers
    });
});

builder.Services.AddEndpointsApiExplorer();

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AuthService API",
        Version = "v1",
        Description = "Authentication & Authorization API with RBAC - Optimized"
    });

    // Enhanced custom schema ID generation
    c.CustomSchemaIds(type =>
    {
        var fullName = type.FullName ?? type.Name;

        fullName = fullName
            .Replace("AuthService.Application.Features.", "App.")
            .Replace("AuthService.Infrastructure.Services.", "Infra.")
            .Replace("AuthService.Domain.Entities.", "Domain.")
            .Replace("AuthService.Api.", "Api.");

        if (type.IsGenericType)
        {
            var genericTypeName = fullName.Split('`')[0];
            var genericArgs = type.GetGenericArguments()
                .Select(t => GetSchemaId(t))
                .ToArray();
            return $"{genericTypeName}_{string.Join("_", genericArgs)}";
        }

        return fullName;
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Helper method
static string GetSchemaId(Type type)
{
    var name = type.Name;

    if (type.FullName != null)
    {
        name = type.FullName
            .Replace("AuthService.Application.Features.", "")
            .Replace("AuthService.Infrastructure.Services.", "")
            .Replace("AuthService.Domain.Entities.", "")
            .Replace("AuthService.Api.", "");
    }

    if (type.IsGenericType)
    {
        var genericTypeName = name.Split('`')[0];
        var genericArgs = type.GetGenericArguments()
            .Select(t => GetSchemaId(t))
            .ToArray();
        return $"{genericTypeName}_{string.Join("_", genericArgs)}";
    }

    return name;
}

// Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);

// Mapster
var config = TypeAdapterConfig.GlobalSettings;
config.Scan(typeof(Program).Assembly);
builder.Services.AddSingleton(config);
builder.Services.AddScoped<IMapper, ServiceMapper>();

// MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(
        typeof(AuthService.Application.Features.Auth.Register.RegisterCommand).Assembly
    )
);

// FluentValidation
builder.Services.AddValidatorsFromAssembly(
    typeof(AuthService.Application.Features.Auth.Register.RegisterCommand).Assembly
);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev-secret-key-change-me";
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ClockSkew = TimeSpan.Zero // Reduce clock skew for more precise token expiration
    };
});

// Role-based policies - Using AddAuthorizationBuilder (fixes ASP0025)
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequireAdmin", policy =>
        policy.RequireRole(Roles.Admin))
    .AddPolicy("RequireUserOrAdmin", policy =>
        policy.RequireRole(Roles.User, Roles.Admin))
    .AddPolicy("RequireSuperAdmin", policy =>
        policy.RequireRole(SystemRoles.SuperAdmin))
    .AddPolicy("RequireFinanceAdmin", policy =>
        policy.RequireRole(Roles.FinanceAdmin, SystemRoles.SuperAdmin));

builder.Services.AddHealthChecks();

var app = builder.Build();

// ============================================
// OPTIMIZATION: Warm up database connection pool
// ============================================
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        logger.LogInformation("Warming up database connection pool...");
        await dbContext.Database.CanConnectAsync();
        logger.LogInformation("Database connection pool warmed up successfully");
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Failed to warm up database connection pool");
    }
}

// Seed database
using (var scope = app.Services.CreateScope())
{
    try
    {
        await DatabaseSeeder.SeedAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AuthService.Api v1");
        // c.RoutePrefix = string.Empty; // optional: serve Swagger UI at root
    });
}

// Global Exception Handler - Always return JSON for API errors
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Unhandled exception occurred");

        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        // Fix collection initialization warnings (IDE0300, CA1861)
        var errorMessage = app.Environment.IsDevelopment() 
            ? ex.Message 
            : "An error occurred while processing your request";
        
        var errorDetails = app.Environment.IsDevelopment()
    ? new List<string> { ex.ToString() }
    : new List<string> { "Internal server error" };

        var errorResponse = new
        {
            success = false,
            message = errorMessage,
            data = (object?)null,
            errors = errorDetails
        };

        await context.Response.WriteAsJsonAsync(errorResponse);
    }
});

// ============================================
// OPTIMIZATION: Middleware order is important for performance
// ============================================
app.UseResponseCompression(); // Must be first
app.UseHttpsRedirection();
app.UseResponseCaching(); // Before static files and CORS
app.UseCors("AllowBlazorClient");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();
