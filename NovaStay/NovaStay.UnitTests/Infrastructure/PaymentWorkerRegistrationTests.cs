using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NovaStay.Infrastructure.Payments;
using NovaStay.Infrastructure.Persistence.DI;

namespace NovaStay.UnitTests.Infrastructure;

public class PaymentWorkerRegistrationTests
{
    [Theory]
    [InlineData(null, true)]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public void SharedDatabaseClientCanDisableBothPaymentWorkersWithoutDisablingOtherWorkers(string? configured, bool expected)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["SQLString"] = "not-used", ["JWT:SecretKey"] = new string('t', 64),
            ["JWT:Issuer"] = "test", ["JWT:Audience"] = "test", ["Payments:BackgroundWorkersEnabled"] = configured
        }).Build();
        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);
        var workers = services.Where(x => x.ServiceType == typeof(IHostedService)).Select(x => x.ImplementationType).ToArray();
        Assert.Equal(expected, workers.Contains(typeof(PaymentOutboxWorker)));
        Assert.Equal(expected, workers.Contains(typeof(PayOsReconciliationWorker)));
        Assert.Contains(workers, worker => worker?.Name == "ContractRenewalNotifierBackgroundService");
    }
}
