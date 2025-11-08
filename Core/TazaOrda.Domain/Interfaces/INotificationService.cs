using TazaOrda.Domain.DTOs.Notifications;
using TazaOrda.Domain.Entities;
using TazaOrda.Domain.Enums;

namespace TazaOrda.Domain.Interfaces;

/// <summary>
/// Интерфейс сервиса для работы с уведомлениями
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Получить список уведомлений пользователя
    /// </summary>
    Task<List<NotificationDto>> GetNotificationsAsync(Guid userId, bool unreadOnly = false, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Отметить уведомление как прочитанное
    /// </summary>
    Task<MarkAsReadResponse> MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Отметить все уведомления как прочитанные
    /// </summary>
    Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить количество непрочитанных уведомлений
    /// </summary>
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Отправить уведомление о создании обращения
    /// </summary>
    Task SendReportCreatedNotificationAsync(Report report, CancellationToken cancellationToken = default);

    /// <summary>
    /// Отправить уведомление об изменении статуса обращения
    /// </summary>
    Task SendReportStatusChangedNotificationAsync(Report report, ReportStatus oldStatus, ReportStatus newStatus, CancellationToken cancellationToken = default);

    /// <summary>
    /// Отправить уведомление о присоединении к акции
    /// </summary>
    Task SendEventJoinedNotificationAsync(Event eventEntity, User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Отправить уведомление о начислении монет
    /// </summary>
    Task SendCoinsAwardedNotificationAsync(User user, decimal amount, string reason, Guid? relatedEntityId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Отправить произвольное уведомление
    /// </summary>
    Task SendNotificationAsync(Notification notification, CancellationToken cancellationToken = default);
}
