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
                options.Configuration = configuration["Redis_ConnectionString"]
                    ?? configuration["Redis:ConnectionString"]
                    ?? throw new InvalidOperationException("Redis_ConnectionString is not configured.");
                options.InstanceName = "NovaStay:";
            });


            services.AddMassTransit(x =>
            {
                x.AddConsumer<NotificationEmailComsumer>();
                x.AddConsumer<NotificationResetPasswordConsumer>();
                x.AddConsumer<NotificationContractRenewConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(configuration["RabbitMQ_HostName"] ?? "localhost", h =>
                    {
                        h.Username(configuration["RabbitMQ_UserName"]!);
                        h.Password(configuration["RabbitMQ_Password"]!);
                    });

                    cfg.ReceiveEndpoint("Notification", e =>  // name of queue
                    {
                        e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                        e.ConfigureConsumer<NotificationEmailComsumer>(context);       // map consumer with queue
                    });

                    cfg.ReceiveEndpoint("NotificationResetPasswordqueue", e =>
                    {
                        e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                        e.ConfigureConsumer<NotificationResetPasswordConsumer>(context);
                    });

                    cfg.ReceiveEndpoint("NotificationContractRenewqueue", e =>
                    {
                        e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                        e.ConfigureConsumer<NotificationContractRenewConsumer>(context);
                    });
                });
            });


            services.AddSingleton(sp =>
            {
                return new MinioClient()
                    .WithEndpoint(configuration["MinIO_Endpoint"] ?? configuration["MinIO:Endpoint"])
                    .WithCredentials(
                        configuration["MinIO_AccessKey"] ?? configuration["MinIO:AccessKey"],
                        configuration["MinIO_SecretKey"] ?? configuration["MinIO:SecretKey"])
                    .WithSSL(bool.TryParse(configuration["MinIO_UseSSL"] ?? configuration["MinIO:UseSSL"], out var useSsl) && useSsl)
                    .Build();
            });


            return services;
        }
    }
}
