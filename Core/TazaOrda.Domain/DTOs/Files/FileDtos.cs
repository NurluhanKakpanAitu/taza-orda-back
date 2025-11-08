namespace TazaOrda.Domain.DTOs.Files;

/// <summary>
/// DTO для ответа после загрузки файла
/// </summary>
public record FileUploadResponse
{
    public string Url { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public long FileSize { get; init; }
    public string ContentType { get; init; } = string.Empty;
}

/// <summary>
/// DTO категории обращения
/// </summary>
public record CategoryDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? IconUrl { get; init; }
}
