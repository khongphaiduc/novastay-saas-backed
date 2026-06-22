using System.ComponentModel.DataAnnotations;

namespace NovaStay.Application.DTOs;

public sealed class LogoutRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
