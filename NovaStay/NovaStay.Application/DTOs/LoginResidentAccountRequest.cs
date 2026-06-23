using System.ComponentModel.DataAnnotations;

namespace NovaStay.Application.DTOs;

public sealed class LoginResidentAccountRequest
{
    [Required]
    [MaxLength(15)]
    public string Sdt { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;
}
