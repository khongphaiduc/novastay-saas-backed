using System.ComponentModel.DataAnnotations;

namespace NovaStay.Application.DTOs;

public sealed class LoginBusinessAccountRequest
{
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;
}
