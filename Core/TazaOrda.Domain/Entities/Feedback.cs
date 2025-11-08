using TazaOrda.Domain.Common;

namespace TazaOrda.Domain.Entities;

/// <summary>
/// Обратная связь/оценка о решении обращения или работе компании
/// </summary>
public class Feedback : BaseEntity
{
    /// <summary>
    /// ID пользователя, оставившего отзыв
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Пользователь (навигационное свойство)
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// ID обращения, к которому относится отзыв (опционально)
    /// </summary>
    public Guid? ReportId { get; set; }

    /// <summary>
    /// Обращение (навигационное свойство)
    /// </summary>
    public Report? Report { get; set; }

    /// <summary>
    /// ID компании, о которой отзыв (опционально)
    /// </summary>
    public Guid? CompanyId { get; set; }

    /// <summary>
    /// Компания (навигационное свойство)
    /// </summary>
    public Company? Company { get; set; }

    /// <summary>
    /// ID акции, о которой отзыв (опционально)
    /// </summary>
    public Guid? EventId { get; set; }

    /// <summary>
    /// Акция (навигационное свойство)
    /// </summary>
    public Event? Event { get; set; }

    /// <summary>
    /// Оценка от 1 до 5 звёзд
    /// </summary>
    public int Rating { get; set; }

    /// <summary>
    /// Комментарий пользователя
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Категория отзыва (например: "Качество работы", "Скорость", "Вежливость")
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Фотографии к отзыву (URLs через запятую или JSON)
    /// </summary>
    public string? PhotoUrls { get; set; }

    /// <summary>
    /// Признак публичности отзыва (показывать ли другим пользователям)
    /// </summary>
    public bool IsPublic { get; set; } = true;

    /// <summary>
    /// Признак одобрения модератором
    /// </summary>
    public bool IsApproved { get; set; } = false;

    /// <summary>
    /// ID модератора, одобривший/отклонивший отзыв
    /// </summary>
    public Guid? ModeratedById { get; set; }

    /// <summary>
    /// Модератор (навигационное свойство)
    /// </summary>
    public User? ModeratedBy { get; set; }

    /// <summary>
    /// Дата модерации
    /// </summary>
    public DateTime? ModeratedAt { get; set; }

    /// <summary>
    /// Комментарий модератора
    /// </summary>
    public string? ModeratorComment { get; set; }

    /// <summary>
    /// Ответ компании/администратора на отзыв
    /// </summary>
    public string? Response { get; set; }

    /// <summary>
    /// ID сотрудника, ответившего на отзыв
    /// </summary>
    public Guid? RespondedById { get; set; }

    /// <summary>
    /// Сотрудник, ответивший на отзыв (навигационное свойство)
    /// </summary>
    public User? RespondedBy { get; set; }

    /// <summary>
    /// Дата ответа на отзыв
    /// </summary>
    public DateTime? RespondedAt { get; set; }

    /// <summary>
    /// Количество лайков/полезностей от других пользователей
    /// </summary>
    public int HelpfulCount { get; set; } = 0;

    /// <summary>
    /// Признак отклонения отзыва (спам, некорректный контент)
    /// </summary>
    public bool IsRejected { get; set; } = false;

    /// <summary>
    /// Причина отклонения
    /// </summary>
    public string? RejectionReason { get; set; }

    /// <summary>
    /// Проверка, является ли отзыв положительным (4-5 звёзд)
    /// </summary>
    public bool IsPositive => Rating >= 4;

    /// <summary>
    /// Проверка, является ли отзыв отрицательным (1-2 звезды)
    /// </summary>
    public bool IsNegative => Rating <= 2;

    /// <summary>
    /// Проверка, есть ли ответ на отзыв
    /// </summary>
    public bool HasResponse => !string.IsNullOrWhiteSpace(Response);

    /// <summary>
    /// Проверка, ожидает ли отзыв модерации
    /// </summary>
    public bool IsPendingModeration => !IsApproved && !IsRejected;

    /// <summary>
    /// Создать отзыв
    /// </summary>
    public static Feedback Create(
        User user,
        int rating,
        string? comment = null,
        Report? report = null,
        Company? company = null,
        Event? eventEntity = null)
    {
        if (rating < 1 || rating > 5)
        {
            throw new ArgumentException("Оценка должна быть от 1 до 5", nameof(rating));
        }

        if (report == null && company == null && eventEntity == null)
        {
            throw new ArgumentException("Отзыв должен быть связан с обращением, компанией или акцией");
        }

        return new Feedback
        {
            UserId = user.Id,
            User = user,
            Rating = rating,
            Comment = comment,
            ReportId = report?.Id,
            Report = report,
            CompanyId = company?.Id,
            Company = company,
            EventId = eventEntity?.Id,
            Event = eventEntity,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Одобрить отзыв
    /// </summary>
    public void Approve(User moderator, string? comment = null)
    {
        if (IsApproved)
        {
            throw new InvalidOperationException("Отзыв уже одобрен");
        }

        IsApproved = true;
        IsRejected = false;
        ModeratedById = moderator.Id;
        ModeratedBy = moderator;
        ModeratedAt = DateTime.UtcNow;
        ModeratorComment = comment;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Отклонить отзыв
    /// </summary>
    public void Reject(User moderator, string reason)
    {
        if (IsRejected)
        {
            throw new InvalidOperationException("Отзыв уже отклонён");
        }

        IsRejected = true;
        IsApproved = false;
        IsPublic = false;
        ModeratedById = moderator.Id;
        ModeratedBy = moderator;
        ModeratedAt = DateTime.UtcNow;
        RejectionReason = reason;
        ModeratorComment = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Добавить ответ на отзыв
    /// </summary>
    public void AddResponse(User responder, string response)
    {
        if (string.IsNullOrWhiteSpace(response))
        {
            throw new ArgumentException("Ответ не может быть пустым", nameof(response));
        }

        if (!responder.IsEmployee && !responder.IsAdmin)
        {
            throw new InvalidOperationException("Только сотрудники и администраторы могут отвечать на отзывы");
        }

        Response = response;
        RespondedById = responder.Id;
        RespondedBy = responder;
        RespondedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Увеличить счётчик полезности
    /// </summary>
    public void IncrementHelpful()
    {
        HelpfulCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Уменьшить счётчик полезности
    /// </summary>
    public void DecrementHelpful()
    {
        if (HelpfulCount > 0)
        {
            HelpfulCount--;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Обновить оценку
    /// </summary>
    public void UpdateRating(int newRating)
    {
        if (newRating < 1 || newRating > 5)
        {
            throw new ArgumentException("Оценка должна быть от 1 до 5", nameof(newRating));
        }

        Rating = newRating;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Скрыть отзыв от публичного просмотра
    /// </summary>
    public void Hide()
    {
        IsPublic = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Показать отзыв публично
    /// </summary>
    public void Show()
    {
        if (IsApproved && !IsRejected)
        {
            IsPublic = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}