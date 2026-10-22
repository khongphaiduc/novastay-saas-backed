using Microsoft.Extensions.Configuration;
using NovaStay.API.Configuration;

namespace NovaStay.UnitTests.API;

public class DevelopmentPaymentConfigurationTests
{
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
