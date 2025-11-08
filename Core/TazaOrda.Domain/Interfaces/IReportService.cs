using TazaOrda.Domain.DTOs.Reports;
using TazaOrda.Domain.Entities;

namespace TazaOrda.Domain.Interfaces;

/// <summary>
/// Интерфейс сервиса для работы с обращениями
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Создать новое обращение
    /// </summary>
    Task<CreateReportResponse> CreateReportAsync(CreateReportRequest request, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить список обращений с фильтрацией
    /// </summary>
    Task<List<ReportListDto>> GetReportsAsync(
        Guid? userId = null,
        string? status = null,
        Guid? districtId = null,
        string? category = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить детальную информацию об обращении
    /// </summary>
    Task<ReportDetailDto?> GetReportByIdAsync(Guid reportId, Guid? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Отправить отзыв об обращении
    /// </summary>
    Task CreateFeedbackAsync(Guid reportId, CreateFeedbackRequest request, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить количество обращений пользователя
    /// </summary>
    Task<int> GetUserReportsCountAsync(Guid userId, CancellationToken cancellationToken = default);
}
