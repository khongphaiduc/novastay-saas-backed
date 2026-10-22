namespace NovaStay.Application.DTOs;

public sealed record BankAccountDto(Guid Id, string BankName, string AccountNumber, string AccountName, bool HasQr);
public sealed record BillingLineDto(string Key, string Description, decimal Quantity, decimal UnitPrice, decimal Amount);
public sealed record BillingRoomDto(Guid RoomId, string RoomNumber, Guid? ResidentId, string? PayerName,
    IReadOnlyList<BillingLineDto> Lines, IReadOnlyList<string> Errors);
public sealed class IssueBillsRequest
{
    public string PaymentMode { get; set; } = "Manual";
    public Guid PropertyId { get; set; }
    public string Period { get; set; } = "";
    public DateOnly DueDate { get; set; }
    public List<Guid> RoomIds { get; set; } = [];
    public List<string> ChargeKeys { get; set; } = [];
}
public sealed record BillDto(Guid Id, Guid? PropertyId, string PropertyName, string RoomNumber, string PayerName,
    string Number, string Period, DateOnly? DueDate, decimal TotalAmount, string Status,
    DateTime? ReportedAt, DateTime? PaidAt, string? ConfirmationNote, BankAccountDto? BankAccount,
    IReadOnlyList<BillingLineDto> Lines, string PaymentMode = "Manual", string? PaymentSource = null);
public sealed record ConfirmBillRequest(string? Note);
