using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using NovaStay.API.Controllers;
using NovaStay.API.Security;
using NovaStay.Application.Services;

namespace NovaStay.UnitTests.API;

public sealed class OwnerGuardAttachmentTests
{
    [Theory]
    [InlineData(typeof(RoomsController))]
    [InlineData(typeof(ContractsController))]
    [InlineData(typeof(ResidentsController))]
    [InlineData(typeof(PropertiesController))]
    [InlineData(typeof(OrganizationsController))]
    [InlineData(typeof(AssetsController))]
    [InlineData(typeof(UtilityMetersController))]
    [InlineData(typeof(ExpensesController))]
    [InlineData(typeof(IncomeReceiptsController))]
    [InlineData(typeof(FinancialTransactionsController))]
    [InlineData(typeof(MaintenanceController))]
    public void OwnerOperations_HaveOwnershipFilter(Type controller) =>
        Assert.Single(controller.GetCustomAttributes(typeof(OwnerDataAttribute), true));

    [Theory]
    [InlineData("http://127.0.0.1/internal")]
    [InlineData("http://169.254.169.254/latest/meta-data/")]
    [InlineData("https://storage.example.invalid.evil.invalid/uploads/a.png")]
    [InlineData("https://storage.example.invalid/not-uploads/a.png")]
    [InlineData("https://storage.example.invalid/uploads/../private/a.png")]
    [InlineData("https://user@storage.example.invalid/uploads/a.png")]
    public async Task ImageProxy_RejectsUntrustedSourcesBeforeNetwork(string url)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?> {
            ["Storage:PublicBaseUrl"] = "https://storage.example.invalid/uploads" }).Build();
        var api = new RoomsController(Mock.Of<IRoomService>());
        Assert.IsType<BadRequestObjectResult>(await api.ProxyImage(url, config, default));
    }
}
