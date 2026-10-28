namespace NovaStay.Application.Common;

public sealed class RoomOccupancyException(string message) : InvalidOperationException(message);
