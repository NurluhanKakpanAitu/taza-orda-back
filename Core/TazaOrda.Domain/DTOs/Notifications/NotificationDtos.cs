namespace TazaOrda.Domain.DTOs.Notifications;

/// <summary>
/// DTO уведомления
/// </summary>
public record NotificationDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public bool IsRead { get; init; }
    public DateTime Date { get; init; }
    public string Type { get; init; } = string.Empty;
    public string? ActionUrl { get; init; }
    public int Priority { get; init; }
}

/// <summary>
/// Ответ на пометку уведомления как прочитанного
/// </summary>
public record MarkAsReadResponse
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
}
