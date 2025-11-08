using TazaOrda.Domain.DTOs.Files;

namespace TazaOrda.Domain.Interfaces;

/// <summary>
/// Интерфейс сервиса для работы с файлами (MinIO)
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Загрузить файл в хранилище
    /// </summary>
    Task<FileUploadResponse> UploadFileAsync(
        Stream fileStream, 
        string fileName, 
        string contentType, 
        long fileSize,
        string folder = "reports", 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить файл из хранилища
    /// </summary>
    Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить публичный URL файла
    /// </summary>
    Task<string> GetFileUrlAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверить существование файла
    /// </summary>
    Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default);
}
