using TazaOrda.Domain.DTOs.Events;

namespace TazaOrda.Domain.DTOs.Reports;

/// <summary>
/// DTO для создания обращения
/// </summary>
public record CreateReportRequest
{
    public string Category { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public double Lat { get; init; }
    public double Lng { get; init; }
    public string? PhotoUrl { get; init; }
    public string? Street { get; init; }
    public Guid? DistrictId { get; init; }
}

/// <summary>
/// DTO списка обращений
/// </summary>
public record ReportListDto
{
    public Guid Id { get; init; }
    public string Category { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public LocationDto Location { get; init; } = null!;
    public string? PhotoUrl { get; init; }
    public string Description { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
}

/// <summary>
/// DTO детальной информации об обращении
/// </summary>
public record ReportDetailDto
{
    public Guid Id { get; init; }
    public string Category { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? PhotoUrl { get; init; } = null;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime? ClosedAt { get; init; }
    public string? AdminComment { get; init; }
    public LocationDto Location { get; init; } = null!;
    public string Address { get; init; } = string.Empty;
    public int LikesCount { get; init; }
    public AuthorDto Author { get; init; } = null!;
    public AssigneeDto? AssignedTo { get; init; }
    public FeedbackDto? UserFeedback { get; init; }
}


/// <summary>
/// DTO автора обращения
/// </summary>
public record AuthorDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
}

/// <summary>
/// DTO исполнителя
/// </summary>
public record AssigneeDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}

/// <summary>
/// DTO обратной связи
/// </summary>
public record FeedbackDto
{
    public int Rating { get; init; }
    public string? Comment { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// DTO для отправки отзыва
/// </summary>
public record CreateFeedbackRequest
{
    public int Rating { get; init; }
    public string? Comment { get; init; }
}

/// <summary>
/// Ответ на создание обращения
/// </summary>
public record CreateReportResponse
{
    public string Message { get; init; } = string.Empty;
    public Guid ReportId { get; init; }
}
