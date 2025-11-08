namespace TazaOrda.Domain.Enums;

/// <summary>
/// Типы уведомлений
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// Уведомление об обращении (новое, изменение статуса и т.д.)
    /// </summary>
    Report = 0,

    /// <summary>
    /// Уведомление об акции/мероприятии
    /// </summary>
    Event = 1,

    /// <summary>
    /// Уведомление о начислении/списании coins
    /// </summary>
    Coin = 2,

    /// <summary>
    /// Уведомление о маршруте
    /// </summary>
    Route = 3,

    /// <summary>
    /// Уведомление о награде
    /// </summary>
    Reward = 4,

    /// <summary>
    /// Системное уведомление
    /// </summary>
    System = 5,

    /// <summary>
    /// Напоминание
    /// </summary>
    Reminder = 6,

    /// <summary>
    /// Уведомление об обновлении профиля
    /// </summary>
    Profile = 7,

    /// <summary>
    /// Уведомление о достижении
    /// </summary>
    Achievement = 8,

    /// <summary>
    /// Другое
    /// </summary>
    Other = 99
}