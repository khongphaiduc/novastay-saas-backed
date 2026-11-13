using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NovaStay.API.Configuration;
using NovaStay.API.Controllers;
using Xunit;

namespace NovaStay.UnitTests.API;

// Kiểm tra tài liệu Swagger chỉ chứa endpoint frontend đang dùng và mọi endpoint trong danh sách đều tồn tại.
public sealed class SwaggerDocumentTests
{
    [Fact]
    public async Task SwaggerDocument_ListsExactlyTheUsedEndpoints()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
        builder.WebHost.UseKestrel().UseUrls("http://127.0.0.1:0");
        builder.Services.AddControllers().AddApplicationPart(typeof(AssetsController).Assembly);
        builder.Services.AddNovaStaySwagger();
        await using var app = builder.Build();
        app.UseNovaStaySwagger();
        app.MapControllers();
        await app.StartAsync();

        using var client = new HttpClient { BaseAddress = new Uri(app.Urls.First()) };
        var json = await client.GetStringAsync("/swagger/v1/swagger.json");
        await app.StopAsync();

        using var document = JsonDocument.Parse(json);
        var documented = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var path in document.RootElement.GetProperty("paths").EnumerateObject())
        {
            foreach (var operation in path.Value.EnumerateObject())
            {
                documented.Add($"{operation.Name.ToUpperInvariant()} {Normalize(path.Name)}");
            }
        }

        var used = (HashSet<string>)typeof(SwaggerConfiguration)
            .GetField("UsedEndpoints", BindingFlags.NonPublic | BindingFlags.Static)!
            .GetValue(null)!;

        var missing = used.Where(endpoint => !documented.Contains(endpoint)).OrderBy(x => x).ToArray();
        var unexpected = documented.Where(endpoint => !used.Contains(endpoint)).OrderBy(x => x).ToArray();

        Assert.True(missing.Length == 0, "Có endpoint trong danh sách nhưng không có trong backend: " + string.Join("; ", missing));
        Assert.True(unexpected.Length == 0, "Có endpoint trong Swagger nhưng không nằm trong danh sách: " + string.Join("; ", unexpected));
        Assert.DoesNotContain(documented, endpoint => endpoint.Contains("invitation-links", StringComparison.OrdinalIgnoreCase));
    }

    private static string Normalize(string path) =>
        Regex.Replace(path.Trim('/'), @"\{[^}]*\}", "{}");
}
