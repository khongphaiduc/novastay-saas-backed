using NovaStay.Application.Common.Validation;
using Xunit;

namespace NovaStay.UnitTests.Application;

public sealed class VietnameseInputAttributesTests
{
    [Theory]
    [InlineData("0912345678")]
    [InlineData("0355555555")]
    public void VietnamesePhone_AcceptsTenDigitsStartingWithZero(string phone)
    {
        Assert.True(new VietnamesePhoneAttribute().IsValid(phone));
    }

    [Theory]
    [InlineData("912345678")]     // thiếu số 0 đầu
    [InlineData("09123456789")]   // 11 số
    [InlineData("091234567")]     // 9 số
    [InlineData("0912-345-678")]  // có ký tự khác
    [InlineData("+84912345678")]
    public void VietnamesePhone_RejectsWrongFormat(string phone)
    {
        Assert.False(new VietnamesePhoneAttribute().IsValid(phone));
    }

    [Fact]
    public void VietnamesePhone_SkipsNullAndBlankSoRequiredCanHandleEmptiness()
    {
        Assert.True(new VietnamesePhoneAttribute().IsValid(null));
        Assert.True(new VietnamesePhoneAttribute().IsValid("   "));
    }

    [Theory]
    [InlineData("012345678901")]
    [InlineData("000000000000")]
    public void CitizenId_AcceptsTwelveDigits(string id)
    {
        Assert.True(new CitizenIdAttribute().IsValid(id));
    }

    [Theory]
    [InlineData("12345678901")]    // 11 số
    [InlineData("1234567890123")]  // 13 số
    [InlineData("12345678901a")]   // có chữ
    public void CitizenId_RejectsWrongFormat(string id)
    {
        Assert.False(new CitizenIdAttribute().IsValid(id));
    }
}
