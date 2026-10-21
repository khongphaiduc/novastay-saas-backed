using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using NovaStay.Application.Services;
using NovaStay.Infrastructure.Persistence.DI;

namespace NovaStay.UnitTests.Infrastructure;

public sealed class JwtConfigurationTests
{
    [Theory]
    [InlineData(":")]
    [InlineData("_")]
    public void TokenPair_UsesConfiguredSigningKeyAndLifetimes(string separator)
    {
        var key = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                [$"JWT{separator}SecretKey"] = key,
                [$"JWT{separator}Issuer"] = "test-issuer",
                [$"JWT{separator}Audience"] = "test-audience",
                [$"JWT{separator}AccessTokenMinutes"] = "17",
                [$"JWT{separator}RefreshTokenDays"] = "3"
            }).Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddInfrastructure(configuration);
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IJwtTokenService>();
        var now = DateTime.UtcNow;

        var pair = service.CreateTokenPair(Guid.NewGuid(), Guid.NewGuid(), "Resident",
            "Test", "", null, now);

        Assert.Equal(now.AddMinutes(17), pair.AccessTokenExpiresAt);
        Assert.Equal(now.AddDays(3), pair.RefreshTokenExpiresAt);
        var segments = pair.AccessToken.Split('.');
        using var payload = JsonDocument.Parse(Base64UrlEncoder.DecodeBytes(segments[1]));
        Assert.Equal("test-issuer", payload.RootElement.GetProperty("iss").GetString());
        Assert.Equal("test-audience", payload.RootElement.GetProperty("aud").GetString());
        Assert.Equal(new DateTimeOffset(pair.AccessTokenExpiresAt).ToUnixTimeSeconds(),
            payload.RootElement.GetProperty("exp").GetInt64());
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var signature = hmac.ComputeHash(Encoding.ASCII.GetBytes(segments[0] + "." + segments[1]));
        Assert.Equal(signature, Base64UrlEncoder.DecodeBytes(segments[2]));
    }
}
