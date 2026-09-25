using NovaStay.Application.Common.Mappings;
using NovaStay.Application.Services;
using DotNetEnv;
using NovaStay.Infrastructure.Persistence.Mapping;
using NovaStay.Infrastructure.Persistence.DI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.HttpOverrides;

namespace NovaStay.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var infrastructureConfigPaths = new[]
            {
                Path.GetFullPath(Path.Combine(
                    builder.Environment.ContentRootPath,
                    "..",
                    "NovaStay.Infrastructure",
                    "Config",
                    "appsettings.json")),
                Path.GetFullPath(Path.Combine(
                    builder.Environment.ContentRootPath,
                    "NovaStay.Infrastructure",
                    "Config",
                    "appsettings.json"))
            };

            foreach (var infrastructureConfigPath in infrastructureConfigPaths)
            {
                builder.Configuration.AddJsonFile(
                    infrastructureConfigPath,
                    optional: true,
                    reloadOnChange: builder.Environment.IsDevelopment());
            }

            var envPaths = new[]
            {
                Path.GetFullPath(Path.Combine(
                    builder.Environment.ContentRootPath,
                    "..",
                    "NovaStay.Infrastructure",
                    "Config",
                    ".env")),
                Path.GetFullPath(Path.Combine(
                    builder.Environment.ContentRootPath,
                    "NovaStay.Infrastructure",
                    "Config",
                    ".env"))
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

            builder.Services.ConfigurePersistenceServices(builder.Configuration); // dependency service of project 

            builder.Services.AddInfrastructure(builder.Configuration);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    var origins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                        ?? new[] { "https://novastay.io.vn", "https://www.novastay.io.vn" };

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

            builder.Services.AddScoped<ISampleDataService, SampleDataService>();

            var jwtSecret = builder.Configuration["JWT:SecretKey"];
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
                            ValidIssuer = builder.Configuration["JWT:Issuer"],
                            ValidAudience = builder.Configuration["JWT:Audience"],
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
                        };
                    });
            }




            builder.Services.AddControllers();

            var app = builder.Build();

            // HTTPS handled by Nginx reverse proxy — only redirect in Development
            if (app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            // Forward headers from Nginx (X-Forwarded-For, X-Forwarded-Proto)
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseCors("AllowFrontend");
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
