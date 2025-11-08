namespace TazaOrda.Domain.Enums;

/// <summary>
/// Каналы доставки уведомлений
/// </summary>
public enum NotificationChannel
{
    /// <summary>
    /// Push-уведомление в мобильном приложении
    /// </summary>
    Push = 0,

    /// <summary>
    /// SMS-сообщение
    /// </summary>
    Sms = 1,

    /// <summary>
    /// Email-письмо
    /// </summary>
    Email = 2,

    /// <summary>
    /// Telegram-сообщение
    /// </summary>
    Telegram = 3,

    /// <summary>
    /// Внутреннее уведомление в системе (только в приложении)
    /// </summary>
    InApp = 4,

    /// <summary>
    /// WhatsApp-сообщение
    /// </summary>
    WhatsApp = 5
}