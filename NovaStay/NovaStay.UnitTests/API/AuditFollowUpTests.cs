using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using NovaStay.API.Controllers;
using NovaStay.API.Security;
using Xunit;

namespace NovaStay.UnitTests.API;

public sealed class AuditFollowUpTests
{
    [Fact]
    public void SampleData_requires_admin()
    {
        var attribute = Assert.Single(typeof(SampleDataController).GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>());
        Assert.Equal("Admin", attribute.Roles);
    }

    [Fact]
    public void Proxy_image_is_rate_limited()
    {
        var method = typeof(RoomsController).GetMethod(nameof(RoomsController.ProxyImage))!;
        var attribute = Assert.Single(method.GetCustomAttributes(typeof(EnableRateLimitingAttribute), true).Cast<EnableRateLimitingAttribute>());
        Assert.Equal(ApiRateLimits.ProxyImage, attribute.PolicyName);
    }
}
