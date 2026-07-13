using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NovaStay.Application.Common.Mappings;
using NovaStay.Application.Services;
using NovaStay.Infrastructure.Persistence.DI;
using NovaStay.Infrastructure.Persistence.Mapping;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using System.Text;

namespace NovaStay.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration().MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
              .WriteTo.Console()   // write log  in console screen 
                .WriteTo.File(
        "logs/log-.txt",
        rollingInterval: RollingInterval.Day)
              .CreateLogger();

            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddSerilog();

            builder.Services.AddOpenTelemetry()
             .ConfigureResource(resource => resource
             .AddService(
                   serviceName: "NovaStay.API",
                   serviceVersion: "1.0.0"))
             .WithTracing(tracing =>
             {
                 tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter();
             })
             .WithMetrics(metrics =>
             {
                 metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter();     // export metrics to OpenTelemetry Collector
             });

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
                    policy
                        .WithOrigins(
                            "http://localhost:5173",
                            "https://novastay.io.vn",
                            "https://www.novastay.io.vn",
                            "https://nestone.io.vn/",
                            "https://www.nestone.io.vn/"
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
            app.UseSerilogRequestLogging();    // middleware of Serilog
            app.UseHttpsRedirection();
            app.UseCors("AllowFrontend");
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
