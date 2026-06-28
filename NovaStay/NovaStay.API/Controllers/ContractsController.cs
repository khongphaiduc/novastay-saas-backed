using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaStay.Application.DTOs;
using NovaStay.Application.Services;
using NovaStay.Infrastructure.ServicesImple;

namespace NovaStay.API.Controllers;

/// <summary>
/// Contract Management: TASK-031 đến TASK-039
/// </summary>
[Authorize(Roles = "BusinessOwner")]
[ApiController]
[Route("api/contracts")]
public sealed class ContractsController : ControllerBase
{
    private readonly IContractService _contractService;
    private readonly IMinioStorageService _storageService;

    public ContractsController(IContractService contractService, IMinioStorageService storageService)
    {
        _contractService = contractService;
        _storageService = storageService;
    }

    /// <summary>TASK-036: Danh sách hợp đồng + TASK-037: Tìm kiếm</summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<ContractDetailDto>>> GetContracts(
        [FromQuery] Guid organizationId,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] Guid? residentId = null,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 12,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var contracts = await _contractService.GetContractsAsync(
                organizationId, search, status, residentId, pageIndex, pageSize, cancellationToken);
            return Ok(contracts);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>TASK-031: Tạo hợp đồng thuê</summary>
    [HttpPost]
    public async Task<ActionResult<ContractDetailDto>> CreateContract(
        [FromBody] CreateContractRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var contract = await _contractService.CreateContractAsync(request, cancellationToken);
            return Created($"/api/contracts/{contract.Id}", contract);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    /// <summary>TASK-032: Cập nhật hợp đồng</summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ContractDetailDto>> UpdateContract(
        Guid id,
        [FromBody] UpdateContractRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var contract = await _contractService.UpdateContractAsync(id, request, cancellationToken);
            return Ok(contract);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    /// <summary>TASK-033: Gia hạn hợp đồng</summary>
    [HttpPost("{id:guid}/renew")]
    public async Task<ActionResult<ContractDetailDto>> RenewContract(
        Guid id,
        [FromBody] RenewContractRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var contract = await _contractService.RenewContractAsync(id, request, cancellationToken);
            return Ok(contract);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    /// <summary>TASK-034: Thanh lý hợp đồng</summary>
    [HttpPost("{id:guid}/terminate")]
    public async Task<ActionResult<ContractDetailDto>> TerminateContract(
        Guid id,
        [FromBody] TerminateContractRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var contract = await _contractService.TerminateContractAsync(id, request, cancellationToken);
            return Ok(contract);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    /// <summary>TASK-035: Upload file hợp đồng PDF</summary>
    [HttpPost("{id:guid}/upload-pdf")]
    public async Task<ActionResult<ContractDetailDto>> UploadContractPdf(
        Guid id,
        IFormFile? file,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "File is required." });

        try
        {
            using var stream = file.OpenReadStream();
            var pdfUrl = await _storageService.UploadImageAsync(stream, file.FileName, file.ContentType, cancellationToken);
            var contract = await _contractService.UploadContractPdfAsync(id, pdfUrl, cancellationToken);
            return Ok(contract);
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
    }

    /// <summary>TASK-038: Hợp đồng sắp hết hạn</summary>
    [HttpGet("expiring-soon")]
    public async Task<ActionResult<IReadOnlyList<ContractDetailDto>>> GetExpiringSoon(
        [FromQuery] Guid organizationId,
        [FromQuery] int days = 30,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var contracts = await _contractService.GetExpiringSoonAsync(organizationId, days, cancellationToken);
            return Ok(contracts);
        }
        catch (ArgumentException ex) { return BadRequest(new { message = ex.Message }); }
    }

    /// <summary>TASK-NEW: Gửi thông báo nhắc nhở gia hạn thủ công</summary>
    [HttpPost("{id:guid}/send-renewal-notice")]
    public async Task<IActionResult> SendRenewalNotice(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _contractService.SendRenewalNotificationAsync(id, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
