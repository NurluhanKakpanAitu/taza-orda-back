using TazaOrda.Domain.Common;
using TazaOrda.Domain.Enums;

namespace TazaOrda.Domain.Entities;

/// <summary>
/// Уведомление пользователю о событиях в системе
/// </summary>
public class Notification : BaseEntity
{
    /// <summary>
    /// ID получателя уведомления
    /// </summary>
    public Guid RecipientId { get; set; }

    /// <summary>
    /// Получатель (навигационное свойство)
    /// </summary>
    public User Recipient { get; set; } = null!;

    /// <summary>
    /// Тип уведомления
    /// </summary>
    public NotificationType Type { get; set; }

    /// <summary>
    /// Заголовок уведомления
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Текст сообщения
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Канал доставки уведомления
    /// </summary>
    public NotificationChannel Channel { get; set; }

    /// <summary>
    /// Признак прочтения уведомления
    /// </summary>
    public bool IsRead { get; set; } = false;

    /// <summary>
    /// Дата и время прочтения
    /// </summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// Признак успешной отправки
    /// </summary>
    public bool IsSent { get; set; } = false;

    /// <summary>
    /// Дата и время отправки
    /// </summary>
    public DateTime? SentAt { get; set; }

    /// <summary>
    /// Количество попыток отправки
    /// </summary>
    public int SendAttempts { get; set; } = 0;

    /// <summary>
    /// Сообщение об ошибке (если отправка не удалась)
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Дата следующей попытки отправки (для неудачных отправок)
    /// </summary>
    public DateTime? NextRetryAt { get; set; }

    /// <summary>
    /// Тип связанного объекта (например: "Report", "Event", "Route")
    /// </summary>
    public string? RelatedEntityType { get; set; }

    /// <summary>
    /// ID связанного объекта
    /// </summary>
    public Guid? RelatedEntityId { get; set; }

    /// <summary>
    /// URL для перехода при нажатии на уведомление (deep link)
    /// </summary>
    public string? ActionUrl { get; set; }

    /// <summary>
    /// Дополнительные данные в формате JSON
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Приоритет уведомления (1 - низкий, 5 - высокий)
    /// </summary>
    public int Priority { get; set; } = 3;

    /// <summary>
    /// Дата истечения актуальности уведомления
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// ID отправителя (если уведомление от пользователя)
    /// </summary>
    public Guid? SenderId { get; set; }

    /// <summary>
    /// Отправитель (навигационное свойство)
    /// </summary>
    public User? Sender { get; set; }

    /// <summary>
    /// Проверка, является ли уведомление непрочитанным
    /// </summary>
    public bool IsUnread => !IsRead;

    /// <summary>
    /// Проверка, истекла ли актуальность уведомления
    /// </summary>
    public bool IsExpired
    {
        get
        {
            if (!ExpiresAt.HasValue)
                return false;

            return DateTime.UtcNow > ExpiresAt.Value;
        }
    }

    /// <summary>
    /// Проверка, требуется ли повторная отправка
    /// </summary>
    public bool RequiresRetry
    {
        get
        {
            if (IsSent || IsExpired)
                return false;

            if (SendAttempts >= 3) // Максимум 3 попытки
                return false;

            if (NextRetryAt.HasValue && DateTime.UtcNow < NextRetryAt.Value)
                return false;

            return true;
        }
    }

    /// <summary>
    /// Отметить уведомление как прочитанное
    /// </summary>
    public void MarkAsRead()
    {
        if (IsRead)
            return;

        IsRead = true;
        ReadAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Отметить уведомление как непрочитанное
    /// </summary>
    public void MarkAsUnread()
    {
        IsRead = false;
        ReadAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Отметить уведомление как отправленное
    /// </summary>
    public void MarkAsSent()
    {
        IsSent = true;
        SentAt = DateTime.UtcNow;
        ErrorMessage = null;
        NextRetryAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Отметить неудачную попытку отправки
    /// </summary>
    public void MarkSendFailed(string errorMessage, int retryDelayMinutes = 5)
    {
        SendAttempts++;
        ErrorMessage = errorMessage;

        if (SendAttempts < 3)
        {
            // Экспоненциальная задержка: 5 мин, 15 мин, 45 мин
            var delayMinutes = retryDelayMinutes * Math.Pow(3, SendAttempts - 1);
            NextRetryAt = DateTime.UtcNow.AddMinutes(delayMinutes);
        }

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Создать уведомление
    /// </summary>
    public static Notification Create(
        User recipient,
        NotificationType type,
        string title,
        string message,
        NotificationChannel channel,
        string? relatedEntityType = null,
        Guid? relatedEntityId = null,
        string? actionUrl = null,
        User? sender = null,
        int priority = 3)
    {
        return new Notification
        {
            RecipientId = recipient.Id,
            Recipient = recipient,
            Type = type,
            Title = title,
            Message = message,
            Channel = channel,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId,
            ActionUrl = actionUrl,
            SenderId = sender?.Id,
            Sender = sender,
            Priority = priority,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Создать уведомление об обращении
    /// </summary>
    public static Notification CreateReportNotification(
        User recipient,
        Report report,
        string message,
        NotificationChannel channel = NotificationChannel.Push)
    {
        return Create(
            recipient,
            NotificationType.Report,
            "Обращение обновлено",
            message,
            channel,
            "Report",
            report.Id,
            $"/reports/{report.Id}",
            priority: report.IsUrgent ? 5 : 3
        );
    }

    /// <summary>
    /// Создать уведомление об акции
    /// </summary>
    public static Notification CreateEventNotification(
        User recipient,
        Event eventEntity,
        string message,
        NotificationChannel channel = NotificationChannel.Push)
    {
        return Create(
            recipient,
            NotificationType.Event,
            eventEntity.Title,
            message,
            channel,
            "Event",
            eventEntity.Id,
            $"/events/{eventEntity.Id}"
        );
    }

    /// <summary>
    /// Создать уведомление о начислении coins
    /// </summary>
    public static Notification CreateCoinNotification(
        User recipient,
        decimal amount,
        string reason,
        NotificationChannel channel = NotificationChannel.Push)
    {
        return Create(
            recipient,
            NotificationType.Coin,
            "Начисление баллов",
            $"Вам начислено {amount} coins. Причина: {reason}",
            channel,
            priority: 4
        );
    }
}