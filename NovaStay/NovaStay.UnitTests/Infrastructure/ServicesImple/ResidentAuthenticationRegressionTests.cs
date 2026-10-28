using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.Common.Mappings;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using NovaStay.Domain.Entities;
using NovaStay.Infrastructure.ContextDB;
using NovaStay.Infrastructure.Persistence.Auth;
using NovaStay.Infrastructure.Persistence.DI;
using NovaStay.Infrastructure.Persistence.Mapping;

namespace NovaStay.UnitTests.Infrastructure.ServicesImple;

public class ResidentAuthenticationRegressionTests
{
    private sealed class GoogleFixture(int parallel = 1) : IGoogleIdentityValidator
    {
        public GoogleIdentity Identity = new("verified-test-subject", "resident@example.invalid", "Test resident");
        private int calls;
        private readonly TaskCompletionSource ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public async Task<GoogleIdentity> ValidateAsync(string token, CancellationToken ct)
        {
            if (Interlocked.Increment(ref calls) >= parallel) ready.TrySetResult();
            await ready.Task.WaitAsync(ct);
            return Identity;
        }
    }
    private static ServiceProvider Provider(GoogleFixture google)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["SQLString"] = "unused", ["JWT:SecretKey"] = new string('t', 64), ["JWT:Issuer"] = "test", ["JWT:Audience"] = "test" }).Build();
        var services = new ServiceCollection().AddLogging().AddSingleton<IConfiguration>(config);
        services.AddAutoMapper(c => { c.AddProfile<ApplicationMappingProfile>(); c.AddProfile<DatabaseModelMappingProfile>(); });
        services.AddInfrastructure(config);
        var database = Guid.NewGuid().ToString();
        services.AddScoped(_ => new HostContext(new DbContextOptionsBuilder<HostContext>().UseInMemoryDatabase(database).Options));
        services.AddSingleton<IGoogleIdentityValidator>(google);
        return services.BuildServiceProvider();
    }
    private static async Task<LoginResidentAccountResponse> GoogleLogin(ServiceProvider provider)
    {
        await using var scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IResidentAuthService>()
            .GoogleLoginAsync(new GoogleLoginResidentRequest { IdToken = "fixture-token" }, null);
    }

    [Fact]
    public async Task FirstGoogleSignupAndRepeatLogin_CreateOneAccountAndResident()
    {
        await using var provider = Provider(new());
        var first = await GoogleLogin(provider); var repeat = await GoogleLogin(provider);
        Assert.Equal(first.AccountId, repeat.AccountId);
        await using var scope = provider.CreateAsyncScope(); var db = scope.ServiceProvider.GetRequiredService<HostContext>();
        Assert.Single(db.Accounts); Assert.Single(db.Residents);
        Assert.Equal(2, await db.AccountRefreshTokens.CountAsync());
        Assert.Single(db.AccountRefreshTokens.Where(x => x.RevokedAt == null));
    }

    [Fact]
    public async Task LinkingExistingEmail_UpdatesAccountOnceWithoutTrackingConflict()
    {
        await using var provider = Provider(new());
        await using (var scope = provider.CreateAsyncScope())
            await scope.ServiceProvider.GetRequiredService<IResidentAuthService>().RegisterAsync(new RegisterResidentAccountRequest
            { FullName = "Test resident", Phone = "0900000001", Email = "resident@example.invalid", Password = "test-only-password" }, null);
        await GoogleLogin(provider);
        await using var verify = provider.CreateAsyncScope(); var db = verify.ServiceProvider.GetRequiredService<HostContext>();
        Assert.Single(db.Accounts); Assert.Single(db.Residents);
        Assert.Equal("verified-test-subject", (await db.Accounts.SingleAsync()).GoogleId);
    }

    [Fact]
    public async Task ConcurrentFirstSignups_ShareOneIdentityWithoutDuplicateAccountOrTrackingErrors()
    {
        await using var provider = Provider(new(8));
        var responses = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => GoogleLogin(provider)));
        Assert.Single(responses.Select(x => x.AccountId).Distinct());
        await using var scope = provider.CreateAsyncScope(); var db = scope.ServiceProvider.GetRequiredService<HostContext>();
        Assert.Single(db.Accounts); Assert.Single(db.Residents);
        Assert.Single(db.AccountRefreshTokens.Where(x => x.RevokedAt == null));
    }

    [Fact]
    public async Task AnotherGoogleIdentityCannotTakeOverAnEmail_AndInactiveAccountsCannotLogin()
    {
        var google = new GoogleFixture(); await using var provider = Provider(google);
        await GoogleLogin(provider);
        google.Identity = google.Identity with { Subject = "different-verified-subject" };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => GoogleLogin(provider));
        await using (var scope = provider.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<HostContext>();
            var account = await db.Accounts.SingleAsync(); Assert.Equal("verified-test-subject", account.GoogleId);
            account.IsActive = false; await db.SaveChangesAsync();
        }
        google.Identity = google.Identity with { Subject = "verified-test-subject" };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => GoogleLogin(provider));
    }

    [Fact]
    public async Task RepositoryRepeatedUpdate_ReusesTrackedEntityAndPreservesAddedState()
    {
        await using var provider = Provider(new()); await using var scope = provider.CreateAsyncScope();
        var unit = scope.ServiceProvider.GetRequiredService<IUnitOfWork>(); var db = scope.ServiceProvider.GetRequiredService<HostContext>();
        var account = new AccountEntity { Id = Guid.NewGuid(), AccountType = "Resident", CustomerName = "First", Phone = "000" };
        await unit.Accounts.AddAsync(account);
        account.CustomerName = "Second"; unit.Accounts.Update(account); unit.Accounts.Update(account);
        var entry = Assert.Single(db.ChangeTracker.Entries<NovaStay.Infrastructure.Models.Account>());
        Assert.Equal(EntityState.Added, entry.State);
        await unit.SaveChangesAsync();
        Assert.Equal("Second", (await db.Accounts.SingleAsync()).CustomerName);
        account.CustomerName = "Third"; unit.Accounts.Update(account); unit.Accounts.Update(account);
        await unit.SaveChangesAsync(); Assert.Single(db.Accounts);
        unit.Accounts.Remove(account); await unit.SaveChangesAsync(); Assert.Empty(db.Accounts);
    }

    [Fact]
    public async Task CancelledWaitingGoogleRequest_DoesNotLeaveIdentityLocked()
    {
        using var coordinator = new GoogleSignInCoordinator();
        using (await coordinator.EnterAsync("same-subject", default))
        {
            using var ct = new CancellationTokenSource(); ct.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => coordinator.EnterAsync("same-subject", ct.Token));
        }
        using var next = await coordinator.EnterAsync("same-subject", default);
    }
}
