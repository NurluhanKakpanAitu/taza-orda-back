using Microsoft.EntityFrameworkCore;
using TazaOrda.Domain.DTOs.Notifications;
using TazaOrda.Domain.Entities;
using TazaOrda.Domain.Enums;
using TazaOrda.Domain.Interfaces;
using TazaOrda.Infrastructure.Persistence;

namespace TazaOrda.Infrastructure.Services.Notifications;

/// <summary>
/// Реализация сервиса для работы с уведомлениями
/// </summary>
public class NotificationService : INotificationService
{
    private readonly TazaOrdaDbContext _context;

    public NotificationService(TazaOrdaDbContext context)
    {
        _context = context;
    }

    public async Task<List<NotificationDto>> GetNotificationsAsync(Guid userId, bool unreadOnly = false, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var query = _context.Notifications
            .Where(n => n.RecipientId == userId);

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                IsRead = n.IsRead,
                Date = n.CreatedAt,
                Type = n.Type.ToString(),
                ActionUrl = n.ActionUrl,
                Priority = n.Priority
            })
            .ToListAsync(cancellationToken);

        return notifications;
    }

    public async Task<MarkAsReadResponse> MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.RecipientId == userId, cancellationToken);

        if (notification == null)
        {
            return new MarkAsReadResponse
            {
                Success = false,
                Message = "Уведомление не найдено"
            };
        }

        notification.MarkAsRead();
        await _context.SaveChangesAsync(cancellationToken);

        return new MarkAsReadResponse
        {
            Success = true,
            Message = "Уведомление отмечено как прочитанное"
        };
    }

    public async Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var unreadNotifications = await _context.Notifications
            .Where(n => n.RecipientId == userId && !n.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var notification in unreadNotifications)
        {
            notification.MarkAsRead();
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .Where(n => n.RecipientId == userId && !n.IsRead)
            .CountAsync(cancellationToken);
    }

    public async Task SendReportCreatedNotificationAsync(Report report, CancellationToken cancellationToken = default)
    {
        // Уведомление для автора обращения
        var notification = Notification.CreateReportNotification(
            recipient: report.Author,
            report: report,
            message: $"Ваше обращение #{report.Id} принято в обработку. Статус: {report.Status}",
            channel: NotificationChannel.Push
        );

        await SendNotificationAsync(notification, cancellationToken);
    }

    public async Task SendReportStatusChangedNotificationAsync(
        Report report,
        ReportStatus oldStatus,
        ReportStatus newStatus,
        CancellationToken cancellationToken = default)
    {
        var statusText = newStatus switch
        {
            ReportStatus.InProgress => "взято в работу",
            ReportStatus.Completed => "выполнено",
            ReportStatus.Closed => "отменено",
            _ => "обновлено"
        };

        var notification = Notification.CreateReportNotification(
            recipient: report.Author,
            report: report,
            message: $"Статус вашего обращения #{report.Id} изменён: {statusText}",
            channel: NotificationChannel.Push
        );

        await SendNotificationAsync(notification, cancellationToken);
    }

    public async Task SendEventJoinedNotificationAsync(Event eventEntity, User user, CancellationToken cancellationToken = default)
    {
        var notification = Notification.CreateEventNotification(
            recipient: user,
            eventEntity: eventEntity,
            message: $"Вы успешно зарегистрировались на акцию \"{eventEntity.Title}\". Начало: {eventEntity.StartAt:dd.MM.yyyy HH:mm}",
            channel: NotificationChannel.Push
        );

        await SendNotificationAsync(notification, cancellationToken);
    }

    public async Task SendCoinsAwardedNotificationAsync(
        User user,
        decimal amount,
        string reason,
        Guid? relatedEntityId = null,
        CancellationToken cancellationToken = default)
    {
        var notification = Notification.CreateCoinNotification(
            recipient: user,
            amount: amount,
            reason: reason,
            channel: NotificationChannel.Push
        );

        if (relatedEntityId.HasValue)
        {
            notification.RelatedEntityId = relatedEntityId;
        }

        await SendNotificationAsync(notification, cancellationToken);
    }

    public async Task SendNotificationAsync(Notification notification, CancellationToken cancellationToken = default)
    {
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(cancellationToken);

        // TODO: Здесь можно добавить отправку через push-уведомления, SMS, email и т.д.
        // В зависимости от notification.Channel
    }
}
