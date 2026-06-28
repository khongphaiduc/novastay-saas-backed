using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;

namespace NovaStay.Infrastructure.BackgroundServices
{
    public class ContractRenewalNotifierBackgroundService : BackgroundService
    {
        private readonly ILogger<ContractRenewalNotifierBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ContractRenewalNotifierBackgroundService(
            ILogger<ContractRenewalNotifierBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ContractRenewalNotifierBackgroundService is starting.");

            // Wait a short time before starting the first run to allow the system to fully boot
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndNotifyExpiringContractsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing CheckAndNotifyExpiringContractsAsync.");
                }

                // Run once every 24 hours
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }

            _logger.LogInformation("ContractRenewalNotifierBackgroundService is stopping.");
        }

        private async Task CheckAndNotifyExpiringContractsAsync(CancellationToken stoppingToken)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

            // Find target date exactly 15 days from now
            var targetDate = DateTime.UtcNow.AddDays(15).Date;
            
            // To do this efficiently, we query the DB
            // We don't have a direct query for "exactly X days", so we get all contracts and filter.
            // Ideally this should be a repository method, but doing it simply here.
            // But we don't have GetActiveContracts globally, so we'll fetch across all organizations?
            // Actually, we don't have a global method to fetch all active contracts.
            // I'll need to create one, or just use context if available.
            // Wait, IUnitOfWork exposes Contracts repository. Let's see what methods it has.
            
            // Let's assume we can add a method GetExpiringContractsOnDateAsync or similar.
            // Or since this is a background service, it has access to the internal DbContext if we cast it, 
            // but we shouldn't. Let's add a method to IContractRepository.
            
            var contracts = await unitOfWork.Contracts.GetAllActiveExpiringOnDateAsync(targetDate, stoppingToken);

            if (contracts == null || contracts.Count == 0)
            {
                _logger.LogInformation($"No contracts found expiring exactly on {targetDate:yyyy-MM-dd}.");
                return;
            }

            int count = 0;
            foreach (var contract in contracts)
            {
                var room = await unitOfWork.Rooms.GetByIdAsync(contract.RoomId, stoppingToken);
                var property = await unitOfWork.Properties.GetByIdAsync(contract.PropertyId, stoppingToken);
                var resident = await unitOfWork.Residents.GetByIdAsync(contract.ResidentId, stoppingToken);

                if (resident != null && !string.IsNullOrEmpty(resident.Email) && room != null && property != null)
                {
                    await publishEndpoint.Publish(new NotificationContractRenewEvent
                    {
                        ContractId = contract.Id,
                        ResidentEmail = resident.Email,
                        ResidentName = resident.FullName,
                        RoomNumber = room.RoomNumber,
                        PropertyName = property.PropertyName,
                        EndDate = contract.EndDate.ToDateTime(TimeOnly.MinValue)
                    }, stoppingToken);
                    count++;
                }
            }

            _logger.LogInformation($"Successfully sent {count} renewal notifications for contracts expiring on {targetDate:yyyy-MM-dd}.");
        }
    }
}
