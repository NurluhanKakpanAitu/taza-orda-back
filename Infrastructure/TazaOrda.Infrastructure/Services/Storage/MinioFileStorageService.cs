using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using TazaOrda.Domain.DTOs.Files;
using TazaOrda.Domain.Interfaces;

namespace TazaOrda.Infrastructure.Services.Storage;

/// <summary>
/// Реализация сервиса для работы с MinIO хранилищем
/// </summary>
public class MinioFileStorageService : IFileStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MinioFileStorageService> _logger;
    private readonly string _bucketName;
    private readonly string _endpoint;
    private readonly bool _useSSL;

    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

    public MinioFileStorageService(
        IMinioClient minioClient,
        IConfiguration configuration,
        ILogger<MinioFileStorageService> logger)
    {
        _minioClient = minioClient;
        _configuration = configuration;
        _logger = logger;
        
        _bucketName = configuration["Minio:Bucket"] ?? "tazaorda";
        _endpoint = configuration["Minio:Endpoint"] ?? "localhost:9000";
        _useSSL = bool.Parse(configuration["Minio:UseSSL"] ?? "false");
    }

    public async Task<FileUploadResponse> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        long fileSize,
        string folder = "reports",
        CancellationToken cancellationToken = default)
    {
        // Валидация файла
        ValidateFile(fileName, contentType, fileSize);

        // Убедиться, что bucket существует
        await EnsureBucketExistsAsync(cancellationToken);

        // Генерация уникального имени файла
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        
        // Формирование пути: folder/YYYY/MM/DD/filename
        var datePath = DateTime.UtcNow.ToString("yyyy/MM/dd");
        var objectName = $"{folder}/{datePath}/{uniqueFileName}";

        try
        {
            // Загрузка файла в MinIO
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName)
                .WithStreamData(fileStream)
                .WithObjectSize(fileSize)
                .WithContentType(contentType);

            await _minioClient.PutObjectAsync(putObjectArgs, cancellationToken);

            _logger.LogInformation("File uploaded successfully: {ObjectName}", objectName);

            // Формирование публичного URL
            var url = await GetFileUrlAsync(objectName, cancellationToken);

            return new FileUploadResponse
            {
                Url = url,
                Path = objectName,
                FileName = uniqueFileName,
                FileSize = fileSize,
                ContentType = contentType
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to MinIO");
            throw new InvalidOperationException("Не удалось загрузить файл", ex);
        }
    }

    public async Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var removeObjectArgs = new RemoveObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(filePath);

            await _minioClient.RemoveObjectAsync(removeObjectArgs, cancellationToken);
            
            _logger.LogInformation("File deleted successfully: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from MinIO: {FilePath}", filePath);
            throw new InvalidOperationException("Не удалось удалить файл", ex);
        }
    }

    public async Task<string> GetFileUrlAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            // Для публичного доступа формируем прямой URL
            // Для приватного - используем presigned URL
            var publicAccess = bool.Parse(_configuration["Minio:PublicAccess"] ?? "false");

            if (publicAccess)
            {
                var protocol = _useSSL ? "https" : "http";
                return $"{protocol}://{_endpoint}/{_bucketName}/{filePath}";
            }
            else
            {
                // Генерация presigned URL (действителен 7 дней)
                var presignedGetObjectArgs = new PresignedGetObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(filePath)
                    .WithExpiry(60 * 60 * 24 * 7); // 7 дней

                return await _minioClient.PresignedGetObjectAsync(presignedGetObjectArgs);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file URL: {FilePath}", filePath);
            throw new InvalidOperationException("Не удалось получить URL файла", ex);
        }
    }

    public async Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var statObjectArgs = new StatObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(filePath);

            await _minioClient.StatObjectAsync(statObjectArgs, cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var bucketExistsArgs = new BucketExistsArgs()
                .WithBucket(_bucketName);

            var exists = await _minioClient.BucketExistsAsync(bucketExistsArgs, cancellationToken);

            if (!exists)
            {
                var makeBucketArgs = new MakeBucketArgs()
                    .WithBucket(_bucketName);

                await _minioClient.MakeBucketAsync(makeBucketArgs, cancellationToken);
                
                _logger.LogInformation("Bucket created: {BucketName}", _bucketName);

                // Установить публичный доступ если нужно
                var publicAccess = bool.Parse(_configuration["Minio:PublicAccess"] ?? "false");
                if (publicAccess)
                {
                    await SetBucketPolicyAsync(cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring bucket exists: {BucketName}", _bucketName);
            throw;
        }
    }

    private async Task SetBucketPolicyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Политика для публичного чтения
            var policy = $$"""
            {
                "Version": "2012-10-17",
                "Statement": [
                    {
                        "Effect": "Allow",
                        "Principal": {"AWS": ["*"]},
                        "Action": ["s3:GetObject"],
                        "Resource": ["arn:aws:s3:::{{_bucketName}}/*"]
                    }
                ]
            }
            """;

            var setPolicyArgs = new SetPolicyArgs()
                .WithBucket(_bucketName)
                .WithPolicy(policy);

            await _minioClient.SetPolicyAsync(setPolicyArgs, cancellationToken);
            
            _logger.LogInformation("Bucket policy set for public access: {BucketName}", _bucketName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not set bucket policy (this is normal if using public MinIO)");
        }
    }

    private void ValidateFile(string fileName, string contentType, long fileSize)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException("Имя файла не может быть пустым");
        }

        if (fileSize == 0)
        {
            throw new ArgumentException("Файл не может быть пустым");
        }

        if (fileSize > MaxFileSize)
        {
            throw new ArgumentException($"Размер файла не должен превышать {MaxFileSize / 1024 / 1024} MB");
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            throw new ArgumentException($"Разрешены только следующие форматы: {string.Join(", ", AllowedExtensions)}");
        }

        // Дополнительная проверка MIME типа
        var allowedMimeTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        if (!allowedMimeTypes.Contains(contentType.ToLowerInvariant()))
        {
            throw new ArgumentException("Недопустимый тип файла");
        }
    }
}
