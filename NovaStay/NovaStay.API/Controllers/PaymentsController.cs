using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovaStay.Infrastructure.Payments;

namespace NovaStay.API.Controllers;

[ApiController]
[Authorize(Roles = "BusinessOwner,Resident")]
public sealed class PaymentsController(PaymentProcessingService service) : ControllerBase
{
    private Guid AccountId => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : throw new UnauthorizedAccessException();

    [HttpGet("api/billing/payment-capabilities"), Authorize(Roles = "BusinessOwner")]
    public Task<IActionResult> Capabilities(CancellationToken ct) => Run(async () => await service.Capabilities(AccountId, ct));

    [HttpGet("api/billing/invoices/{id:guid}/payment-status")]
    public Task<IActionResult> Status(Guid id, CancellationToken ct) => Run(async () => await service.Status(AccountId, id, ct));

    [HttpPost("api/billing/invoices/{id:guid}/payment-link")]
    public Task<IActionResult> CreateLink(Guid id, CancellationToken ct) => Run(async () => await service.CreateLink(AccountId, id, ct));

    [HttpPost("api/billing/invoices/{id:guid}/reconcile"), Authorize(Roles = "BusinessOwner")]
    public Task<IActionResult> Reconcile(Guid id, CancellationToken ct) => Run(async () => await service.Reconcile(AccountId, id, ct));

    [AllowAnonymous, HttpPost("api/payments/payos/webhook"), RequestSizeLimit(64 * 1024)]
    public Task<IActionResult> Webhook([FromBody] JsonElement payload, CancellationToken ct) =>
        Run(async () => new { received = true, matched = await service.Webhook(payload, ct) }, true);

    private async Task<IActionResult> Run(Func<Task<object>> action, bool webhook = false)
    {
        try { return Ok(await action()); }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (KeyNotFoundException) { return NotFound(new { code = "INVOICE_NOT_FOUND", message = "Không tìm thấy hóa đơn." }); }
        catch (PaymentRuleException e)
        {
            var status = e.Code == "PRO_UPGRADE_REQUIRED" ? 403 : e.Code == "INVALID_WEBHOOK_SIGNATURE" ? 400 : 409;
            return StatusCode(status, new { code = e.Code, message = e.Message });
        }
        catch (ArgumentException e) { return BadRequest(new { code = "INVALID_PAYMENT_DATA", message = e.Message }); }
        catch (PaymentProviderException e) { return StatusCode(503, new { code = e.Code, message = e.Message }); }
        catch (DbUpdateException)
        {
            return StatusCode(webhook ? 503 : 409, new { code = "PAYMENT_CONCURRENT_UPDATE", message = "Giao dịch đang được xử lý. Vui lòng tải lại trạng thái và thử lại sau." });
        }
    }
}
