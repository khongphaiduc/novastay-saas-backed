using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovaStay.Application.DTOs;
using NovaStay.Infrastructure.ServicesImple;
using NovaStay.Infrastructure.Payments;

namespace NovaStay.API.Controllers;

[ApiController]
[Authorize]
[Route("api/billing")]
public sealed class BillingController(BillingService service) : ControllerBase
{
    private Guid AccountId => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
        ? id : throw new UnauthorizedAccessException("Phiên đăng nhập không hợp lệ.");

    [HttpGet("bank-account"), Authorize(Roles = "BusinessOwner")]
    public Task<IActionResult> Bank(CancellationToken ct) => Run(async () => new JsonResult(await service.GetBank(AccountId, ct)));

    [HttpPut("bank-account"), Authorize(Roles = "BusinessOwner"), RequestSizeLimit(3 * 1024 * 1024)]
    public Task<IActionResult> SaveBank([FromForm] BankAccountForm form, CancellationToken ct) => Run(async () =>
    {
        byte[]? image = null;
        if (form.QrImage != null)
        {
            if (form.QrImage.Length > 2 * 1024 * 1024) throw new ArgumentException("Ảnh QR tối đa 2 MB.");
            using var stream = new MemoryStream();
            await form.QrImage.CopyToAsync(stream, ct);
            image = stream.ToArray();
        }
        return Ok(await service.SaveBank(AccountId, form.BankName, form.AccountNumber, form.AccountName,
            image, form.QrImage?.ContentType, form.RemoveQr, ct));
    });

    [HttpGet("properties"), Authorize(Roles = "BusinessOwner")]
    public Task<IActionResult> Properties(CancellationToken ct) => Run(async () => Ok(await service.Properties(AccountId, ct)));

    [HttpGet("preview"), Authorize(Roles = "BusinessOwner")]
    public Task<IActionResult> Preview(Guid propertyId, string period, CancellationToken ct) =>
        Run(async () => Ok(await service.Preview(AccountId, propertyId, period, ct)));

    [HttpPost("invoices"), Authorize(Roles = "BusinessOwner")]
    public Task<IActionResult> Issue(IssueBillsRequest request, CancellationToken ct) =>
        Run(async () => Ok(await service.Issue(AccountId, request, ct)));

    [HttpGet("invoices"), Authorize(Roles = "BusinessOwner")]
    public Task<IActionResult> OwnerInvoices(Guid? propertyId, CancellationToken ct) =>
        Run(async () => Ok(await service.OwnerBills(AccountId, propertyId, ct)));

    [HttpGet("invoices/paged"), Authorize(Roles = "BusinessOwner")]
    public Task<IActionResult> OwnerInvoicesPaged(Guid? propertyId = null, int pageIndex = 1, int pageSize = 10,
        string? status = null, string? period = null, string? search = null, CancellationToken ct = default) =>
        Run(async () => Ok(await service.OwnerBillsPaged(AccountId, propertyId, pageIndex, pageSize, status, period, search, ct)));

    [HttpPost("invoices/{id:guid}/confirm"), Authorize(Roles = "BusinessOwner")]
    public Task<IActionResult> Confirm(Guid id, ConfirmBillRequest request, CancellationToken ct) =>
        Run(async () => Ok(await service.Confirm(AccountId, id, request.Note, ct)));

    [HttpGet("me/invoices"), Authorize(Roles = "Resident")]
    public Task<IActionResult> MyInvoices(CancellationToken ct) => Run(async () => Ok(await service.ResidentBills(AccountId, ct)));

    [HttpPost("me/invoices/{id:guid}/report-transfer"), Authorize(Roles = "Resident")]
    public Task<IActionResult> Report(Guid id, CancellationToken ct) => Run(async () => Ok(await service.Report(AccountId, id, ct)));

    [HttpGet("bank-accounts/{id:guid}/qr")]
    public Task<IActionResult> Qr(Guid id, CancellationToken ct) => Run(async () =>
    {
        var (image, type) = await service.Qr(AccountId, id, ct);
        Response.Headers.CacheControl = "private, no-store";
        Response.Headers.XContentTypeOptions = "nosniff";
        return File(image, type, "QR-chuyen-khoan" + (type == "image/png" ? ".png" : ".jpg"));
    });

    private async Task<IActionResult> Run(Func<Task<IActionResult>> action)
    {
        try { return await action(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (KeyNotFoundException e) { return NotFound(new { message = e.Message }); }
        catch (PaymentRuleException e) { return StatusCode(e.Code == "PRO_UPGRADE_REQUIRED" ? 403 : 409, new { code = e.Code, message = e.Message }); }
        catch (ArgumentException e) { return BadRequest(new { message = e.Message }); }
        catch (InvalidOperationException e) { return BadRequest(new { message = e.Message }); }
        catch (DbUpdateConcurrencyException) { return Conflict(new { message = "Hóa đơn vừa thay đổi. Vui lòng tải lại để kiểm tra trạng thái." }); }
        catch (DbUpdateException) { return Conflict(new { message = "Khoản thu hoặc thanh toán đã được ghi nhận. Vui lòng tải lại trước khi thử tiếp." }); }
    }
}

public sealed class BankAccountForm
{
    public string BankName { get; set; } = "";
    public string AccountNumber { get; set; } = "";
    public string AccountName { get; set; } = "";
    public IFormFile? QrImage { get; set; }
    public bool RemoveQr { get; set; }
}
