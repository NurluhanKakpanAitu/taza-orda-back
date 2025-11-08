using TazaOrda.Domain.Common;
using TazaOrda.Domain.Enums;
using TazaOrda.Domain.ValueObjects;

namespace TazaOrda.Domain.Entities;

/// <summary>
/// Обращение/заявка от жителя о проблеме с чистотой
/// </summary>
public class Report : BaseEntity
{
    /// <summary>
    /// ID автора обращения
    /// </summary>
    public Guid AuthorId { get; set; }

    /// <summary>
    /// Автор обращения (навигационное свойство)
    /// </summary>
    public User Author { get; set; } = null!;

    /// <summary>
    /// Категория обращения
    /// </summary>
    public ReportCategory Category { get; set; }

    /// <summary>
    /// Описание проблемы
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Геолокация проблемы
    /// </summary>
    public Location Location { get; set; } = null!;

    /// <summary>
    /// ID района, к которому относится обращение
    /// </summary>
    public Guid DistrictId { get; set; }

    /// <summary>
    /// Район (навигационное свойство)
    /// </summary>
    public District District { get; set; } = null!;

    /// <summary>
    /// ID контейнера, к которому относится обращение (опционально)
    /// </summary>
    public Guid? ContainerId { get; set; }

    /// <summary>
    /// Контейнер, к которому относится обращение (навигационное свойство)
    /// </summary>
    public Container? Container { get; set; }

    /// <summary>
    /// Название улицы
    /// </summary>
    public string? Street { get; set; }

    /// <summary>
    /// Полный адрес
    /// </summary>
    public string Address => string.IsNullOrWhiteSpace(Street) && string.IsNullOrWhiteSpace(District.Name)
        ? "Адрес не указан"
        : $"{District.Name}, {Street}".Trim(',', ' ');

    /// <summary>
    /// Статус обращения
    /// </summary>
    public ReportStatus Status { get; set; } = ReportStatus.New;

    /// <summary>
    /// Приоритет обращения
    /// </summary>
    public ReportPriority Priority { get; set; } = ReportPriority.Medium;

    /// <summary>
    /// Дата закрытия обращения
    /// </summary>
    public DateTime? ClosedAt { get; set; }

    /// <summary>
    /// ID исполнителя (если назначен)
    /// </summary>
    public Guid? AssignedToId { get; set; }

    /// <summary>
    /// Исполнитель обращения (навигационное свойство)
    /// </summary>
    public User? AssignedTo { get; set; }

    /// <summary>
    /// URL фотографии проблемы
    /// </summary>
    public string? PhotoUrl { get; set; }
    
    public string? PhotoAfterUrl { get; set; }

    /// <summary>
    /// Комментарий исполнителя/администратора
    /// </summary>
    public string? AdminComment { get; set; }

    /// <summary>
    /// Количество лайков/поддержки от других пользователей
    /// </summary>
    public int LikesCount { get; set; } = 0;

    /// <summary>
    /// Коллекция отзывов об обращении
    /// </summary>
    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    /// <summary>
    /// Признак срочности (вычисляется автоматически на основе приоритета)
    /// </summary>
    public bool IsUrgent => Priority is ReportPriority.High or ReportPriority.Critical;

    /// <summary>
    /// Проверка, находится ли обращение в работе
    /// </summary>
    public bool IsInProgress => Status == ReportStatus.InProgress;

    /// <summary>
    /// Проверка, завершено ли обращение
    /// </summary>
    public bool IsCompleted => Status is ReportStatus.Completed or ReportStatus.Closed;

    /// <summary>
    /// Проверка, отклонено ли обращение
    /// </summary>
    public bool IsRejected => Status == ReportStatus.Rejected;

    /// <summary>
    /// Количество дней с момента создания
    /// </summary>
    public int DaysSinceCreation => (DateTime.UtcNow - CreatedAt).Days;

    /// <summary>
    /// Назначить исполнителя
    /// </summary>
    public void AssignTo(User executor)
    {
        AssignedToId = executor.Id;
        AssignedTo = executor;
        Status = ReportStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Завершить обращение
    /// </summary>
    public void Complete(string? comment = null)
    {
        Status = ReportStatus.Completed;
        ClosedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(comment))
        {
            AdminComment = comment;
        }
    }

    /// <summary>
    /// Отклонить обращение
    /// </summary>
    public void Reject(string reason)
    {
        Status = ReportStatus.Rejected;
        ClosedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        AdminComment = reason;
    }
}