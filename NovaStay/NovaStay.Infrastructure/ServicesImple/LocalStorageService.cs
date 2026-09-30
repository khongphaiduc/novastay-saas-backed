using Microsoft.Extensions.Configuration;

namespace NovaStay.Infrastructure.ServicesImple;

/// <summary>
/// Local file storage implementation that replaces MinIO.
/// Files are saved to a local directory and served via Nginx.
/// </summary>
public sealed class LocalStorageService : IMinioStorageService
{
    private readonly string _storagePath;
    private readonly string _publicBaseUrl;

    public LocalStorageService(IConfiguration configuration)
    {
        // Default to /app/uploads inside container, mounted as a volume
        _storagePath = configuration["Storage:LocalPath"] ?? "/opt/novastay/uploads";
        _publicBaseUrl = configuration["Storage:PublicBaseUrl"] ?? configuration["MinIO:PublicEndpoint"] ?? "https://nestone.io.vn/uploads";

        // Ensure directory exists
        Directory.CreateDirectory(_storagePath);
    }

    public async Task<string> UploadImageAsync(
        Stream imageStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName);
        var objectName = $"rooms/{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(_storagePath, objectName);

        // Ensure subdirectory exists
        var dir = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        await imageStream.CopyToAsync(fileStream, cancellationToken);

        return $"{_publicBaseUrl.TrimEnd('/')}/{objectName}";
    }
}
