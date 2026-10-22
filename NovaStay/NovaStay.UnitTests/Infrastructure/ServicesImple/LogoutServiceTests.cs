using Microsoft.EntityFrameworkCore;
using Moq;
using NovaStay.Application.Common.Interfaces;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using NovaStay.Infrastructure.ContextDB;
using NovaStay.Infrastructure.Models;
using NovaStay.Infrastructure.ServicesImple;

namespace NovaStay.UnitTests.Infrastructure.ServicesImple;

public class LogoutServiceTests
{
    [Fact]
    public async Task ResidentWithoutOwnedOrganization_CanLogout_OnlyRequestedSessionIsRevoked()
    {
        using var db = new HostContext(new DbContextOptionsBuilder<HostContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var jwt = new Mock<IJwtTokenService>();
        jwt.Setup(x => x.HashRefreshToken("test-session")).Returns("test-hash");
        var account = new Account { Id = Guid.NewGuid(), AccountType = "Resident", CustomerName = "Test resident", Phone = "000", PasswordHash = "test" };
        var current = new AccountRefreshToken { Id = Guid.NewGuid(), AccountId = account.Id, TokenHash = "test-hash", ExpiresAt = DateTime.UtcNow.AddDays(1) };
        var other = new AccountRefreshToken { Id = Guid.NewGuid(), AccountId = account.Id, TokenHash = "other-hash", ExpiresAt = DateTime.UtcNow.AddDays(1) };
        db.AddRange(account, current, other);
        await db.SaveChangesAsync();
        var service = new LogoutService(jwt.Object, Mock.Of<IUnitOfWork>(), db);
        await service.LogoutAsync(new LogoutRequest { RefreshToken = " test-session " }, "127.0.0.1");
        Assert.NotNull(current.RevokedAt);
        Assert.Equal("127.0.0.1", current.RevokedByIp);
        Assert.Null(other.RevokedAt);
        var revokedAt = current.RevokedAt;
        await service.LogoutAsync(new LogoutRequest { RefreshToken = "test-session" }, "another-ip");
        Assert.Equal(revokedAt, current.RevokedAt);
        Assert.Equal("127.0.0.1", current.RevokedByIp);
        await service.LogoutAsync(new LogoutRequest { RefreshToken = "unknown-session" }, null);
        await service.LogoutAsync(new LogoutRequest { RefreshToken = " " }, null);
        Assert.Null(other.RevokedAt);
    }
}
