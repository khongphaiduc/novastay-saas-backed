using System.ComponentModel.DataAnnotations;

namespace NovaStay.Application.DTOs;

public sealed class GoogleLoginResidentRequest
{
    [Required]
    public string IdToken { get; set; } = string.Empty;
}
