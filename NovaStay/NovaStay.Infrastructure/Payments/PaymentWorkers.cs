using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using NovaStay.Infrastructure.ContextDB;

namespace NovaStay.Infrastructure.Payments;

public sealed class PaymentOutboxWorker(IServiceScopeFactory scopes, ILogger<PaymentOutboxWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        do
        {
            try { await Dispatch(stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception e) { logger.LogWarning("Payment notification dispatch will retry ({ErrorType}).", e.GetType().Name); }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    public async Task Dispatch(CancellationToken ct)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HostContext>();
        var notifier = scope.ServiceProvider.GetRequiredService<IPaymentNotifier>();
        var now = DateTime.UtcNow;
        var pending = await db.PaymentNotificationOutbox.Where(x => x.SentAt == null && (x.NextAttemptAt == null || x.NextAttemptAt <= now))
            .OrderBy(x => x.CreatedAt).Take(50).ToListAsync(ct);
        foreach (var item in pending)
        {
            item.LastAttemptAt = DateTime.UtcNow; item.Attempts++;
            item.NextAttemptAt = DateTime.UtcNow.AddSeconds(Math.Min(300, 5 * Math.Pow(2, Math.Min(6, item.Attempts))));
            await db.SaveChangesAsync(ct);
            try
            {
                await notifier.NotifyAsync(item.OwnerAccountId, item.ResidentAccountId,
                    new InvoicePaymentUpdated(item.Id, item.InvoiceId, "Paid", item.PaymentSource, DateTime.SpecifyKind(item.PaidAt, DateTimeKind.Utc)), ct);
                item.SentAt = DateTime.UtcNow;
                await db.SaveChangesAsync(ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch (Exception e) { logger.LogWarning("Payment notification will retry ({ErrorType}).", e.GetType().Name); }
        }
    }
}

public sealed class PayOsReconciliationWorker(IServiceScopeFactory scopes, PayOsOptions options, ILogger<PayOsReconciliationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(60));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            if (string.IsNullOrWhiteSpace(options.ApiKey) || string.IsNullOrWhiteSpace(options.ChecksumKey)) continue;
            try
            {
                using var scope = scopes.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<HostContext>();
                var cutoff = DateTime.UtcNow.AddSeconds(-60);
                var pending = await db.PayOsPaymentAttempts.AsNoTracking()
                    .Where(x => x.IsActive && x.ChannelOwnerAccountId == options.DemoOwnerAccountId && x.UpdatedAt < cutoff &&
                        (x.Status == "Pending" || x.Status == "Creating" || x.Status == "NeedsReview" || x.Status == "CancellationPending"))
                    .OrderBy(x => x.UpdatedAt).Take(25).Select(x => new { x.InvoiceId, x.ChannelOwnerAccountId }).ToListAsync(stoppingToken);
                foreach (var item in pending)
                {
                    using var taskScope = scopes.CreateScope();
                    try
                    {
                        await taskScope.ServiceProvider.GetRequiredService<PaymentProcessingService>()
                            .Reconcile(item.ChannelOwnerAccountId, item.InvoiceId, stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { throw; }
                    catch (Exception e)
                    {
                        logger.LogWarning("PayOS reconciliation will retry ({ErrorType}).", e.GetType().Name);
                        // Move failed work behind other invoices so one bad channel cannot starve the batch.
                        using var retryScope = scopes.CreateScope();
                        var retryDb = retryScope.ServiceProvider.GetRequiredService<HostContext>();
                        var retries = await retryDb.PayOsPaymentAttempts.Where(x => x.InvoiceId == item.InvoiceId && x.IsActive).ToListAsync(stoppingToken);
                        foreach (var retry in retries) retry.UpdatedAt = DateTime.UtcNow;
                        await retryDb.SaveChangesAsync(stoppingToken);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception e) { logger.LogWarning("PayOS reconciliation batch will retry ({ErrorType}).", e.GetType().Name); }
        }
    }
}
