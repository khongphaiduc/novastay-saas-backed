using Microsoft.Extensions.Configuration;
using NovaStay.API.Configuration;

namespace NovaStay.UnitTests.API;

public class DevelopmentPaymentConfigurationTests
{
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void LocalPaymentWorkersSwitchOnlyOverridesDevelopment(bool development, bool expected)
    {
        var builder = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["Payments:BackgroundWorkersEnabled"] = "true", ["PayOS:ApiKey"] = "existing-channel" });
        DevelopmentPaymentConfiguration.Apply(builder, development, new Dictionary<string, string>
        { ["Payments__BackgroundWorkersEnabled"] = "false" });
        Assert.Equal(expected, builder.Build().GetValue<bool>("Payments:BackgroundWorkersEnabled"));
        Assert.Equal("existing-channel", builder.Build()["PayOS:ApiKey"]);
    }

    [Theory]
    [InlineData(true, "local-channel")]
    [InlineData(false, "inherited-channel")]
    public void LocalPayOsOverridesInheritedChannelOnlyInDevelopment(bool development, string expected)
    {
        var builder = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["PayOS:ClientId"] = "inherited-channel", ["SQLString"] = "inherited-database"
        });
        DevelopmentPaymentConfiguration.Apply(builder, development, new Dictionary<string, string>
        {
            ["PayOS__ClientId"] = "local-channel", ["SQLString"] = "local-database"
        });
        var config = builder.Build();
        Assert.Equal(expected, config["PayOS:ClientId"]);
        Assert.Equal("inherited-database", config["SQLString"]);
    }

    [Fact]
    public void MissingLocalKeyCannotFallBackToAnotherChannelsInheritedKey()
    {
        var builder = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["PayOS:ApiKey"] = "inherited-api-key"
        });
        DevelopmentPaymentConfiguration.Apply(builder, true, new Dictionary<string, string>
        {
            ["PayOS__ClientId"] = "local-channel"
        });
        Assert.Null(builder.Build()["PayOS:ApiKey"]);
    }
}
