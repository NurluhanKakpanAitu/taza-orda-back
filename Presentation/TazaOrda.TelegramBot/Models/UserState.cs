using TazaOrda.Domain.Enums;

namespace TazaOrda.TelegramBot.Models;

/// <summary>
/// Состояние пользователя в диалоге с ботом
/// </summary>
public class UserState
{
    public long TelegramId { get; set; }
    public Guid? UserId { get; set; }
    public ConversationState State { get; set; } = ConversationState.None;
    public Dictionary<string, object> Data { get; set; } = new();
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Состояния диалога
/// </summary>
public enum ConversationState
{
    None,
    AwaitingRegistrationName,
    AwaitingRegistrationPhone,
    AwaitingReportCategory,
    AwaitingReportDescription,
    AwaitingReportLocation,
    AwaitingReportPhoto,
    BrowsingEvents,
    AwaitingEventSubscription
}

/// <summary>
/// Данные для создания обращения
/// </summary>
public class ReportData
{
    public ReportCategory? Category { get; set; }
    public string? Description { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Address { get; set; }
}

/// <summary>
/// Данные для регистрации
/// </summary>
public class RegistrationData
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
}