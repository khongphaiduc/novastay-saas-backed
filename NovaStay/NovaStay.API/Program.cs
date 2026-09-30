using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NovaStay.Application.Common.Mappings;
using NovaStay.Infrastructure.ContextDB;
using NovaStay.Infrastructure.Persistence.DI;
using NovaStay.Infrastructure.Persistence.Mapping;
using System.Text;

namespace NovaStay.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ── Load appsettings from Infrastructure ───────────────────────────────
            var infrastructureConfigPaths = new[]
            {
                Path.GetFullPath(Path.Combine(
                    builder.Environment.ContentRootPath, "..", "NovaStay.Infrastructure", "Config", "appsettings.json")),
                Path.GetFullPath(Path.Combine(
                    builder.Environment.ContentRootPath, "NovaStay.Infrastructure", "Config", "appsettings.json"))
            };

            foreach (var configPath in infrastructureConfigPaths)
            {
                builder.Configuration.AddJsonFile(
                    configPath,
                    optional: true,
                    reloadOnChange: builder.Environment.IsDevelopment());
            }

            // ── Load .env file ─────────────────────────────────────────────────────
            var envPaths = new[]
            {
                Path.GetFullPath(Path.Combine(
                    builder.Environment.ContentRootPath, "..", "NovaStay.Infrastructure", "Config", ".env")),
                Path.GetFullPath(Path.Combine(
                    builder.Environment.ContentRootPath, "NovaStay.Infrastructure", "Config", ".env"))
            };

            foreach (var envPath in envPaths)
            {
                if (File.Exists(envPath))
                {
                    Env.Load(envPath);
                    break;
                }
            }

            builder.Configuration.AddEnvironmentVariables();

            // ── Services ───────────────────────────────────────────────────────────
            builder.Services.ConfigurePersistenceServices(builder.Configuration);
            builder.Services.AddInfrastructure(builder.Configuration);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    var origins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                        ?? new[] { "https://nestone.io.vn", "https://www.nestone.io.vn" };

                    if (builder.Environment.IsDevelopment())
                    {
                        origins = origins.Append("http://localhost:5173").ToArray();
                    }

                    policy
                        .WithOrigins(origins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            builder.Services.AddAutoMapper(configuration =>
            {
                configuration.AddProfile<ApplicationMappingProfile>();
                configuration.AddProfile<DatabaseModelMappingProfile>();
            });

            // ── JWT ────────────────────────────────────────────────────────────────
            var jwtSecret = builder.Configuration["JWT_SecretKey"]
                         ?? builder.Configuration["JWT:SecretKey"];

            if (!string.IsNullOrWhiteSpace(jwtSecret))
            {
                builder.Services
                    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = builder.Configuration["JWT_Issuer"] ?? builder.Configuration["JWT:Issuer"],
                            ValidAudience = builder.Configuration["JWT_Audience"] ?? builder.Configuration["JWT:Audience"],
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
                        };
                    });
            }

            builder.Services.AddControllers();

            var app = builder.Build();

            // ── Database migration ─────────────────────────────────────────────────
            using (var scope = app.Services.CreateScope())
            {
                try
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<HostContext>();
                    await dbContext.Database.MigrateAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[FATAL] Database migration failed: {ex.Message}");
                    throw;
                }
            }

            // ── Middleware pipeline ────────────────────────────────────────────────

            // Forward headers from Nginx — must be FIRST
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            // HTTPS handled by Nginx in production — only redirect in Development
            if (app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseCors("AllowFrontend");
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            await app.RunAsync();
        }
    }
}
