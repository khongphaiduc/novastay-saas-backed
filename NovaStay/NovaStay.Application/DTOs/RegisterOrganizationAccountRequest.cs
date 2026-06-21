using System.ComponentModel.DataAnnotations;

namespace NovaStay.Application.DTOs;

public sealed class RegisterOrganizationAccountRequest
{
    [Required]
    [MaxLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    [Phone]
    [MaxLength(15)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string BusinessArea { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string BusinessName { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;
}
