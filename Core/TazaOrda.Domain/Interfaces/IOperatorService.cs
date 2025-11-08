using TazaOrda.Domain.DTOs.Operator;
using TazaOrda.Domain.Enums;

namespace TazaOrda.Domain.Interfaces;

/// <summary>
/// Интерфейс сервиса для операторской панели
/// </summary>
public interface IOperatorService
{
    /// <summary>
    /// Получить список обращений с фильтрацией и пагинацией
    /// </summary>
    Task<PagedOperatorReportsResponse> GetReportsAsync(OperatorReportFilterParams filterParams, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить детали обращения
    /// </summary>
    Task<OperatorReportDetailDto?> GetReportByIdAsync(Guid reportId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Изменить статус обращения
    /// </summary>
    Task<bool> UpdateReportStatusAsync(Guid reportId, Guid operatorId, ReportStatus status, string? comment, string? photoAfterUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Добавить фото "после" к обращению
    /// </summary>
    Task<bool> AddPhotoAfterAsync(Guid reportId, string photoUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить статистику за период
    /// </summary>
    Task<OperatorStatsDto> GetStatsAsync(string period = "today", CancellationToken cancellationToken = default);

    /// <summary>
    /// Назначить обращение оператору
    /// </summary>
    Task<bool> AssignReportAsync(Guid reportId, Guid operatorId, CancellationToken cancellationToken = default);
}
