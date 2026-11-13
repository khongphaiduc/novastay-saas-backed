using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace NovaStay.Application.Common.Validation;

// Số điện thoại Việt Nam: đúng 10 chữ số, bắt đầu bằng số 0.
// Giá trị null hoặc khoảng trắng được bỏ qua (dùng cùng [Required] nếu trường bắt buộc).
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class VietnamesePhoneAttribute : ValidationAttribute
{
    public const string Pattern = "^0[0-9]{9}$";

    private static readonly Regex PhoneRegex = new(Pattern, RegexOptions.CultureInvariant);

    public VietnamesePhoneAttribute()
        : base("Số điện thoại phải gồm đúng 10 chữ số và bắt đầu bằng số 0.")
    {
    }

    public override bool IsValid(object? value)
    {
        if (value is null) return true;
        if (value is not string text) return false;
        var trimmed = text.Trim();
        if (trimmed.Length == 0) return true;
        return PhoneRegex.IsMatch(trimmed);
    }
}

// Số CCCD: đúng 12 chữ số.
// Giá trị null hoặc khoảng trắng được bỏ qua (dùng cùng [Required] nếu trường bắt buộc).
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class CitizenIdAttribute : ValidationAttribute
{
    public const string Pattern = "^[0-9]{12}$";

    private static readonly Regex IdRegex = new(Pattern, RegexOptions.CultureInvariant);

    public CitizenIdAttribute()
        : base("Số CCCD phải gồm đúng 12 chữ số.")
    {
    }

    public override bool IsValid(object? value)
    {
        if (value is null) return true;
        if (value is not string text) return false;
        var trimmed = text.Trim();
        if (trimmed.Length == 0) return true;
        return IdRegex.IsMatch(trimmed);
    }
}
