using NovaStay.Domain.Enums;

namespace NovaStay.Application.DTOs;

public sealed class UpdateApprovalStatusRequest
{
    public ApprovalStatus Status { get; set; }
}
