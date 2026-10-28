using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace NovaStay.API.Security;

public sealed class SafeApiExceptionHandler(ILogger<SafeApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception error, CancellationToken ct)
    {
        if (error is OperationCanceledException && context.RequestAborted.IsCancellationRequested) return true;
        logger.LogError(error, "Request failed. TraceId={TraceId}", context.TraceIdentifier);
        context.Response.StatusCode = error is DbUpdateException ? 409 : 500;
        await context.Response.WriteAsJsonAsync(new
        {
            code = error is DbUpdateException ? "CONCURRENT_CHANGE" : "INTERNAL_ERROR",
            message = error is DbUpdateException
                ? "Dữ liệu vừa thay đổi hoặc đã tồn tại. Vui lòng tải lại và thử lại."
                : "Không thể xử lý yêu cầu lúc này. Vui lòng thử lại sau.",
            traceId = context.TraceIdentifier
        }, ct);
        return true;
    }
}

public static class SafeAuthErrors
{
    private static readonly HashSet<string> Expected =
    [
        "Phone or email already exists.", "Owner email already exists.", "Email is required.",
        "Số điện thoại đã được sử dụng.", "Email đã được sử dụng.", "Email đã được sử dụng cho tài khoản khác.",
        "Không tìm thấy thông tin cư dân.", "Tài khoản đang được xử lý. Vui lòng thử lại.",
        "Password fields are required.", "New password and confirmation password do not match.",
        "New password must be at least 8 characters.", "New password must be different from current password."
    ];
    public static bool IsExpected(InvalidOperationException error) => Expected.Contains(error.Message);
}
