using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NovaStay.UnitTests.API;

internal static class TestControllerContext
{
    public static ControllerContext WithHttpContext()
    {
        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    public static ControllerContext WithUser(Guid accountId)
    {
        return WithUser(accountId.ToString());
    }

    public static ControllerContext WithUser(string accountIdClaim)
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, accountIdClaim)],
            "Test");

        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };
    }
}
