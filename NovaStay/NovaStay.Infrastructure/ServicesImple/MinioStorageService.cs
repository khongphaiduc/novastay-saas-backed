using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;

namespace NovaStay.Infrastructure.ServicesImple;

public interface IMinioStorageService
{
    Task<string> UploadImageAsync(Stream imageStream, string fileName, string contentType, CancellationToken cancellationToken = default);
}

public sealed class MinioStorageService : IMinioStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly string _bucketName;
    private readonly string _publicEndpoint;

    public MinioStorageService(IConfiguration configuration)
    {
        var endpoint = configuration["MinIO:Endpoint"] ?? "localhost:9000";
        var accessKey = configuration["MinIO:AccessKey"] ?? string.Empty;
        var secretKey = configuration["MinIO:SecretKey"] ?? string.Empty;
        var useSSL = bool.TryParse(configuration["MinIO:UseSSL"], out var ssl) && ssl;

        _bucketName = configuration["MinIO:BucketName"] ?? "novastay-rooms";
        _publicEndpoint = configuration["MinIO:PublicEndpoint"] ?? $"http://{endpoint}";

        _minioClient = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey)
            .WithSSL(useSSL)
            .Build();
    }

    public async Task<string> UploadImageAsync(
        Stream imageStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var bucketExists = await _minioClient.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(_bucketName), cancellationToken);

        if (!bucketExists)
        {
            await _minioClient.MakeBucketAsync(
                new MakeBucketArgs().WithBucket(_bucketName), cancellationToken);

            // Cấu hình bucket public để có thể truy cập ảnh trực tiếp
            var policy = $$"""
            {
                "Version": "2012-10-17",
                "Statement": [{
                    "Effect": "Allow",
                    "Principal": {"AWS": ["*"]},
                    "Action": ["s3:GetObject"],
                    "Resource": ["arn:aws:s3:::{{_bucketName}}/*"]
                }]
            }
            """;

            await _minioClient.SetPolicyAsync(
                new SetPolicyArgs().WithBucket(_bucketName).WithPolicy(policy),
                cancellationToken);
        }

        var extension = Path.GetExtension(fileName);
        var objectName = $"rooms/{Guid.NewGuid():N}{extension}";

        await _minioClient.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName)
                .WithStreamData(imageStream)
                .WithObjectSize(imageStream.Length)
                .WithContentType(contentType),
            cancellationToken);

        return $"{_publicEndpoint}/{_bucketName}/{objectName}";
    }
}
