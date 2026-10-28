using System.ComponentModel.DataAnnotations;

namespace NovaStay.Application.DTOs;

public sealed record RoomOccupantDto(Guid ResidentId, string FullName, string Phone, bool IsRepresentative);
public sealed record RoomOccupancyDto(Guid ContractId, Guid RoomId, string RoomNumber, Guid PropertyId,
    string PropertyName, int MaxOccupants, IReadOnlyList<RoomOccupantDto> Occupants);
public sealed class SetCoResidentsRequest
{
    [Required, MaxLength(50)]
    public List<Guid> ResidentIds { get; set; } = [];
}
