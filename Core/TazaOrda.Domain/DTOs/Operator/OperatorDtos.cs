using TazaOrda.Domain.Enums;

namespace TazaOrda.Domain.DTOs.Operator;

/// <summary>
/// Запрос на изменение статуса обращения
/// </summary>
public record UpdateReportStatusRequest
{
    public ReportStatus Status { get; init; }
    public string? OperatorComment { get; init; }
    
    public string? PhotoAfterUrl { get; init; }
}

/// <summary>
/// Детальная информация об обращении для оператора
/// </summary>
public record OperatorReportDetailDto
{
    public Guid Id { get; init; }
    public ReportCategory Category { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public ReportStatus Status { get; init; }
    public string Description { get; init; } = string.Empty;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public string? Street { get; init; }
    public string? PhotoBefore { get; init; }
    public string? PhotoAfter { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public string? OperatorComment { get; init; }
    public int? Rating { get; init; }
    public string? UserFeedback { get; init; }
    
    // Информация о пользователе
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string UserPhone { get; init; } = string.Empty;
    
    // Информация о районе
    public Guid? DistrictId { get; init; }
    public string? DistrictName { get; init; }
    
    // Информация об операторе
    public Guid? AssignedOperatorId { get; init; }
    public string? AssignedOperatorName { get; init; }
}

/// <summary>
/// Краткая информация об обращении для списка
/// </summary>
public record OperatorReportListDto
{
    public Guid Id { get; init; }
    public ReportCategory Category { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public ReportStatus Status { get; init; }
    public string Description { get; init; } = string.Empty;
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public string? Street { get; init; }
    public DateTime CreatedAt { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string? DistrictName { get; init; }
    public bool HasPhotoBefore { get; init; }
    public bool HasPhotoAfter { get; init; }
}

/// <summary>
/// Пагинированный список обращений
/// </summary>
public record PagedOperatorReportsResponse
{
    public List<OperatorReportListDto> Reports { get; init; } = new();
    public int Total { get; init; }
    public int Page { get; init; }
    public int Size { get; init; }
    public int TotalPages { get; init; }
}

/// <summary>
/// Статистика для панели оператора
/// </summary>
public record OperatorStatsDto
{
    public int Total { get; init; }
    public int New { get; init; }
    public int InProgress { get; init; }
    public int Done { get; init; }
    public int Cancelled { get; init; }
    public string Period { get; init; } = string.Empty;
    public DateTime FromDate { get; init; }
    public DateTime ToDate { get; init; }
}

/// <summary>
/// Параметры фильтрации обращений для оператора
/// </summary>
public record OperatorReportFilterParams
{
    public ReportStatus? Status { get; init; }
    public Guid? DistrictId { get; init; }
    public ReportCategory? Category { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 20;
    public string? SearchTerm { get; init; }
}
