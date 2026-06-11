using Host.Application.Common.Mappings;
using Host.Application.Services;
using DotNetEnv;
using Host.Infrastructure.Persistence.Mapping;
using Host.Infrastructure.Persistence.DI;
using Microsoft.Extensions.Configuration;

namespace Host.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var envPath = Path.GetFullPath(Path.Combine(
                builder.Environment.ContentRootPath,
                "..",
                "Host.Infrastructure",
                "Config",
                ".env"));

            if (File.Exists(envPath))
            {
                Env.Load(envPath);
                builder.Configuration.AddEnvironmentVariables();
            }

            builder.Services.AddInfrastructure(builder.Configuration);

            builder.Services.AddAutoMapper(configuration =>
            {
                configuration.AddProfile<ApplicationMappingProfile>();
                configuration.AddProfile<DatabaseModelMappingProfile>();
            });

            builder.Services.AddScoped<ISampleDataService, SampleDataService>();

            builder.Services.AddControllers();

            var app = builder.Build();



            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
