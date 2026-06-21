using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Minio;
using NovaStay.Infrastructure.Consumers;
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


            services.AddMassTransit(x =>
            {

                x.AddConsumer<NotificationEmailComsumer>();


                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(configuration["RabbitMQ:HostName"] ?? "157.66.219.130", h =>
                    {
                        h.Username(configuration["RabbitMQ:Username"]!);
                        h.Password(configuration["RabbitMQ:Password"]!);
                    });

                    cfg.ReceiveEndpoint("Notification", e =>  // name of queue
                    {
                        e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                        e.ConfigureConsumer<NotificationEmailComsumer>(context);       // map consumer with queue
                    });
                });
            });


            services.AddSingleton(sp =>
            {
                return new MinioClient()
                    .WithEndpoint(configuration["MinIO:Endpoint"])
                    .WithCredentials(
                        configuration["MinIO:AccessKey"],
                        configuration["MinIO:SecretKey"])
                    .WithSSL(bool.Parse(configuration["MinIO:UseSSL"]!))
                    .Build();
            });


            return services;
        }
    }
}
