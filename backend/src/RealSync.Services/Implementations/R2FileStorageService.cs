using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using RealSync.Core.Interfaces;
using RealSync.Services.Options;
using RealSync.Shared.DTOs.Requests;
using RealSync.Shared.Exceptions;

namespace RealSync.Services.Implementations;

public class R2FileStorageService
{
    private readonly R2StorageOptions _options;

    public R2FileStorageService(IConfiguration configuration)
    {
        _options = configuration.GetSection("R2Storage").Get<R2StorageOptions>() ?? new R2StorageOptions();
    }

    public async Task<FileStorageResult> UploadAsync(
        string key,
        FileUploadRequest file,
        CancellationToken cancellationToken = default)
    {
        using var s3Client = CreateClient();
        await using var stream = file.OpenReadStream();
        var request = new PutObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key,
            InputStream = stream,
            ContentType = file.ContentType,
            AutoCloseStream = false,
            UseChunkEncoding = false,
            DisableDefaultChecksumValidation = true,
            DisablePayloadSigning = true
        };

        await s3Client.PutObjectAsync(request, cancellationToken);

        return new FileStorageResult
        {
            Key = key,
            Url = BuildPublicUrl(key)
        };
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        using var s3Client = CreateClient();
        var request = new DeleteObjectRequest
        {
            BucketName = _options.BucketName,
            Key = key.TrimStart('/')
        };

        await s3Client.DeleteObjectAsync(request, cancellationToken);
    }

    private IAmazonS3 CreateClient()
    {
        if (!_options.IsConfigured)
            throw new BusinessException("R2Storage chưa được cấu hình đầy đủ.");

        var credentials = new BasicAWSCredentials(_options.AccessKeyId!, _options.SecretAccessKey!);
        var s3Config = new AmazonS3Config
        {
            ServiceURL = _options.Endpoint,
            ForcePathStyle = true,
            AuthenticationRegion = "auto"
        };

        return new AmazonS3Client(credentials, s3Config);
    }

    private string BuildPublicUrl(string key)
    {
        var normalizedKey = key.TrimStart('/');
        if (!string.IsNullOrWhiteSpace(_options.PublicBaseUrl))
            return $"{_options.PublicBaseUrl.TrimEnd('/')}/{normalizedKey}";

        return $"{_options.Endpoint!.TrimEnd('/')}/{_options.BucketName}/{normalizedKey}";
    }
}

public class FileStorageResult
{
    public string Key { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
