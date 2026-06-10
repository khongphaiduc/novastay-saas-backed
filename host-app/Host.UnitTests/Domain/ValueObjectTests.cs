using Host.Domain.ValueObject;

namespace Host.UnitTests.Domain;

public class ValueObjectTests
{
    [Fact]
    public void Money_Converts_To_Decimal()
    {
        Money money = 125.50m;

        decimal amount = money;

        Assert.Equal(125.50m, amount);
    }

    [Fact]
    public void EmailAddress_Converts_To_String()
    {
        EmailAddress emailAddress = "owner@example.com";

        string value = emailAddress;

        Assert.Equal("owner@example.com", value);
    }
}
