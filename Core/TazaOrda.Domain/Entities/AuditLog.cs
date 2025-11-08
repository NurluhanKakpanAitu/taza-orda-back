using TazaOrda.Domain.Common;
using TazaOrda.Domain.Enums;

namespace TazaOrda.Domain.Entities;

/// <summary>
/// Журнал аудита - фиксация всех действий в системе
/// </summary>
public class AuditLog : BaseEntity
{
    /// <summary>
    /// ID пользователя, выполнившего действие (null для системных действий)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Пользователь (навигационное свойство)
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Тип действия
    /// </summary>
    public AuditAction Action { get; set; }

    /// <summary>
    /// Тип объекта (например: "Report", "User", "Event")
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// ID объекта, над которым выполнено действие
    /// </summary>
    public Guid? EntityId { get; set; }

    /// <summary>
    /// Название/идентификатор объекта (для удобства чтения)
    /// </summary>
    public string? EntityName { get; set; }

    /// <summary>
    /// Описание действия
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Старые значения (JSON) - до изменения
    /// </summary>
    public string? OldValues { get; set; }

    /// <summary>
    /// Новые значения (JSON) - после изменения
    /// </summary>
    public string? NewValues { get; set; }

    /// <summary>
    /// IP-адрес пользователя
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User Agent (браузер/приложение)
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Дата и время выполнения действия
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Дополнительные метаданные (JSON)
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Признак успешности операции
    /// </summary>
    public bool IsSuccess { get; set; } = true;

    /// <summary>
    /// Сообщение об ошибке (если операция не успешна)
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Длительность выполнения операции (в миллисекундах)
    /// </summary>
    public long? DurationMs { get; set; }

    /// <summary>
    /// Имя пользователя (кэшированное для быстрого доступа)
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// Проверка, является ли действие системным
    /// </summary>
    public bool IsSystemAction => UserId == null;

    /// <summary>
    /// Проверка, является ли операция изменением данных
    /// </summary>
    public bool IsDataModification => Action is AuditAction.Created or AuditAction.Updated or AuditAction.Deleted;

    /// <summary>
    /// Создать запись аудита для создания объекта
    /// </summary>
    public static AuditLog LogCreated(
        string entityType,
        Guid entityId,
        string? entityName,
        User? user,
        string? newValues = null,
        string? ipAddress = null)
    {
        return new AuditLog
        {
            Action = AuditAction.Created,
            EntityType = entityType,
            EntityId = entityId,
            EntityName = entityName,
            UserId = user?.Id,
            User = user,
            UserName = user?.FullName,
            NewValues = newValues,
            IpAddress = ipAddress,
            Description = $"Создан объект {entityType}: {entityName}",
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Создать запись аудита для обновления объекта
    /// </summary>
    public static AuditLog LogUpdated(
        string entityType,
        Guid entityId,
        string? entityName,
        User? user,
        string? oldValues = null,
        string? newValues = null,
        string? ipAddress = null)
    {
        return new AuditLog
        {
            Action = AuditAction.Updated,
            EntityType = entityType,
            EntityId = entityId,
            EntityName = entityName,
            UserId = user?.Id,
            User = user,
            UserName = user?.FullName,
            OldValues = oldValues,
            NewValues = newValues,
            IpAddress = ipAddress,
            Description = $"Обновлён объект {entityType}: {entityName}",
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Создать запись аудита для удаления объекта
    /// </summary>
    public static AuditLog LogDeleted(
        string entityType,
        Guid entityId,
        string? entityName,
        User? user,
        string? oldValues = null,
        string? ipAddress = null)
    {
        return new AuditLog
        {
            Action = AuditAction.Deleted,
            EntityType = entityType,
            EntityId = entityId,
            EntityName = entityName,
            UserId = user?.Id,
            User = user,
            UserName = user?.FullName,
            OldValues = oldValues,
            IpAddress = ipAddress,
            Description = $"Удалён объект {entityType}: {entityName}",
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Создать запись аудита для входа в систему
    /// </summary>
    public static AuditLog LogLogin(User user, string? ipAddress = null, string? userAgent = null)
    {
        return new AuditLog
        {
            Action = AuditAction.Login,
            EntityType = "User",
            EntityId = user.Id,
            EntityName = user.FullName,
            UserId = user.Id,
            User = user,
            UserName = user.FullName,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Description = $"Пользователь {user.FullName} вошёл в систему",
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Создать запись аудита для выхода из системы
    /// </summary>
    public static AuditLog LogLogout(User user, string? ipAddress = null)
    {
        return new AuditLog
        {
            Action = AuditAction.Logout,
            EntityType = "User",
            EntityId = user.Id,
            EntityName = user.FullName,
            UserId = user.Id,
            User = user,
            UserName = user.FullName,
            IpAddress = ipAddress,
            Description = $"Пользователь {user.FullName} вышел из системы",
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Отметить операцию как неуспешную
    /// </summary>
    public void MarkAsFailed(string errorMessage)
    {
        IsSuccess = false;
        ErrorMessage = errorMessage;
    }
}