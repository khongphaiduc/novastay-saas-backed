using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using NovaStay.Application.Common.Mappings;
using NovaStay.Application.Services;
using NovaStay.Infrastructure.ContextDB;
using NovaStay.Infrastructure.Persistence.DI;
using NovaStay.Infrastructure.Persistence.Mapping;
using OpenTelemetry.Exporter;
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
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Mvc", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .WriteTo.Console()
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog();

            // ── OpenTelemetry ──────────────────────────────────────────────────────
            builder.Services.AddOpenTelemetry()
                .ConfigureResource(resource => resource
                    .AddService(serviceName: "NovaStay.API", serviceVersion: "1.0.0"))
                .WithTracing(tracing =>
                {
                    tracing
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddConsoleExporter()
                        .AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri("http://157.66.219.130:4317");
                            options.Protocol = OtlpExportProtocol.Grpc;
                        });
                })
                .WithMetrics(metrics =>
                {
                    metrics
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation()
                        .AddConsoleExporter()
                        .AddOtlpExporter(options =>
                        {
                            options.Endpoint = new Uri("http://157.66.219.130:4317");
                            options.Protocol = OtlpExportProtocol.Grpc;
                        });
                });

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

            builder.Services.AddScoped<ISampleDataService, SampleDataService>();

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

            // ── Database migration & seeding ───────────────────────────────────────
            using (var scope = app.Services.CreateScope())
            {
                try
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<HostContext>();
                    await dbContext.Database.MigrateAsync();
                    Log.Information("Database migration completed successfully.");

                    var sampleDataService = scope.ServiceProvider.GetRequiredService<ISampleDataService>();
                    var affectedRows = await sampleDataService.CreatePackageAsync();
                    Log.Information("Subscription packages seeded. Affected rows: {AffectedRows}", affectedRows);
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, "An error occurred while migrating or seeding the database.");
                    throw;
                }
            }

            // ── Middleware pipeline ────────────────────────────────────────────────

            // Forward headers from Nginx (X-Forwarded-For, X-Forwarded-Proto)
            // Must be FIRST before any middleware that reads host/scheme
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseSerilogRequestLogging();

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
