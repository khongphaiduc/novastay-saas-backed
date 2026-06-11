using Host.Infrastructure;
using Host.Application.Common.Mappings;
using Host.Application.Services;
using DotNetEnv;

namespace Host.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Env.Load(Path.Combine(AppContext.BaseDirectory, "Config", ".env"));

            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddInfrastructure(builder.Configuration);

            builder.Services.AddAutoMapper(configuration =>
                configuration.AddProfile<ApplicationMappingProfile>());
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
