using Host.Infrastructure;
using Host.Application.Common.Mappings;

namespace Host.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            builder.Services.AddInfrastructure(builder.Configuration);

            builder.Services.AddAutoMapper(configuration =>
                configuration.AddProfile<ApplicationMappingProfile>());

            builder.Services.AddControllers();

            var app = builder.Build();



            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
