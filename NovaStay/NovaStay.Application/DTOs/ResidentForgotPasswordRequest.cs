using System.ComponentModel.DataAnnotations;
using NovaStay.Application.Common.Validation;

namespace NovaStay.Application.DTOs;

public sealed class ResidentForgotPasswordRequest
{
    [Required]
    [VietnamesePhone]
    public string Sdt { get; set; } = string.Empty;
}
