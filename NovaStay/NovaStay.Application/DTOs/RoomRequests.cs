namespace NovaStay.Application.DTOs;

public sealed class CreateRoomRequest
{
    public Guid PropertyId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int Floor { get; set; }
    public decimal BasePrice { get; set; }
    public string Status { get; set; } = "Available";
    public int? MaxOccupants { get; set; }
    public string? AmenitiesJson { get; set; }
}

public sealed class UpdateRoomRequest
{
    public string RoomNumber { get; set; } = string.Empty;
    public int Floor { get; set; }
    public decimal BasePrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? MaxOccupants { get; set; }
    public string? AmenitiesJson { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

public sealed class UpdateBasePriceRequest
{
    public decimal BasePrice { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

public sealed class UpdateMaxOccupantsRequest
{
    public int MaxOccupants { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

public sealed class UpdateAmenitiesRequest
{
    public string? AmenitiesJson { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

public sealed class UploadRoomImageRequest
{
    public Stream ImageStream { get; set; } = Stream.Null;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "image/jpeg";
    public bool IsCover { get; set; } = false;
}
