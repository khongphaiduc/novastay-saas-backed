using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovaStay.Infrastructure.Persistence.DI
{
    public static class ConfigServicesDependency
    {
        public static IServiceCollection ConfigurePersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration["Redis:ConnectionString"];
                options.InstanceName = "NovaStay:";
            });


            services.AddSingleton(sp =>
            {
                return new MinioClient()
                    .WithEndpoint(configuration["Endpoint"])
                    .WithCredentials(
                        configuration["AccessKey"],
                        configuration["SecretKey"])
                    .WithSSL(bool.Parse(configuration["UseSSL"]!))
                    .Build();
            });


            return services;
        }
    }
}
