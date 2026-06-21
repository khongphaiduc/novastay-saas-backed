using NovaStay.Application.Common.Mappings;
using NovaStay.Application.Services;
using DotNetEnv;
using NovaStay.Infrastructure.Persistence.Mapping;
using NovaStay.Infrastructure.Persistence.DI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace NovaStay.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var infrastructureConfigPath = Path.GetFullPath(Path.Combine(
                builder.Environment.ContentRootPath,
                "..",
                "NovaStay.Infrastructure",
                "Config",
                "appsettings.json"));

            builder.Configuration.AddJsonFile(
                infrastructureConfigPath,
                optional: true,
                reloadOnChange: true);

            var envPath = Path.GetFullPath(Path.Combine(
                builder.Environment.ContentRootPath,
                "..",
                "NovaStay.Infrastructure",
                "Config",
                ".env"));

            if (File.Exists(envPath))
            {
                Env.Load(envPath);
                builder.Configuration.AddEnvironmentVariables();

            }

            builder.Services.ConfigurePersistenceServices(builder.Configuration); // dependency service of project 

            builder.Services.AddInfrastructure(builder.Configuration);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy
                        .WithOrigins(
                            "http://localhost:5173",
                            "https://novastay.io.vn",
                            "https://www.novastay.io.vn"
                        )
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

            var jwtSecret = builder.Configuration["Jwt:Secret"];
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
                            ValidIssuer = builder.Configuration["Jwt:Issuer"],
                            ValidAudience = builder.Configuration["Jwt:Audience"],
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
                        };
                    });
            }




            builder.Services.AddControllers();

            var app = builder.Build();

            app.UseHttpsRedirection();
            app.UseCors("AllowFrontend");
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
