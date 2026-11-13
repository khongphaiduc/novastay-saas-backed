using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NovaStay.API.Controllers;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using NovaStay.Infrastructure.ContextDB;
using NovaStay.Infrastructure.Models;

namespace NovaStay.UnitTests.API;

public sealed class OwnerHttpBoundaryTests
{
    public sealed class TestIdentity(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Guid.TryParse(Request.Headers["X-Test-Owner"], out var id)) return Task.FromResult(AuthenticateResult.NoResult());
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, id.ToString()), new Claim(ClaimTypes.Role, "BusinessOwner") }, "Test"));
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, "Test")));
        }
    }
    [Fact]
    public async Task RealMvcPipeline_RequiresAuthenticationAndChecksOwnerBeforeReadWriteOrUpload()
    {
        await using var db = new HostContext(new DbContextOptionsBuilder<HostContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var owner = Guid.NewGuid(); var organization = Guid.NewGuid(); var property = Guid.NewGuid(); var room = Guid.NewGuid();
        db.AddRange(new Organization { Id = organization, OwnerAccountId = owner, BusinessName = "Test", BusinessArea = "Test", OwnerEmail = "test@example.invalid", OwnerPhone = "000", SubscriptionStatus = "Trial" },
            new Property { Id = property, OrganizationId = organization, PropertyName = "Test", Address = "Test", PropertyType = "BoardingHouse", Status = "Active" },
            new Room { Id = room, PropertyId = property, RoomNumber = "P1", Status = "Available", RowVersion = [] });
        await db.SaveChangesAsync();
        var service = new Mock<IRoomService>();
        service.Setup(x => x.GetRoomsAsync(property, null, null, 1, 12, It.IsAny<CancellationToken>())).ReturnsAsync(new PagedResult<RoomDto>());
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Testing", ContentRootPath = AppContext.BaseDirectory });
        builder.Logging.ClearProviders(); builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddSingleton(db); builder.Services.AddSingleton(service.Object);
        builder.Services.AddControllers().AddApplicationPart(typeof(RoomsController).Assembly);
        builder.Services.AddAuthentication("Test").AddScheme<AuthenticationSchemeOptions,TestIdentity>("Test", _ => {}); builder.Services.AddAuthorization();
        await using var app = builder.Build(); app.UseAuthentication(); app.UseAuthorization(); app.MapControllers(); await app.StartAsync();
        using var client = new HttpClient { BaseAddress = new Uri(app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single()) };
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync($"/api/rooms?propertyId={property}")).StatusCode);
        client.DefaultRequestHeaders.Add("X-Test-Owner", Guid.NewGuid().ToString());
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/rooms?propertyId={property}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PutAsJsonAsync($"/api/rooms/{room}", new { roomNumber="P1", floor=1, basePrice=3000000, status="Available", maxOccupants=1 })).StatusCode);
        using var form = new MultipartFormDataContent(); form.Add(new ByteArrayContent([1,2,3]), "image", "test.png");
        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsync($"/api/rooms/{room}/images", form)).StatusCode);
        client.DefaultRequestHeaders.Remove("X-Test-Owner"); client.DefaultRequestHeaders.Add("X-Test-Owner", owner.ToString());
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/rooms?propertyId={property}")).StatusCode);
        service.Verify(x => x.GetRoomsAsync(property,null,null,1,12,It.IsAny<CancellationToken>()),Times.Once);
        service.Verify(x => x.UpdateRoomAsync(It.IsAny<Guid>(), It.IsAny<UpdateRoomRequest>(), It.IsAny<CancellationToken>()),Times.Never);
        service.Verify(x => x.UploadRoomImageAsync(It.IsAny<Guid>(), It.IsAny<UploadRoomImageRequest>(), It.IsAny<CancellationToken>()),Times.Never);
    }
}
