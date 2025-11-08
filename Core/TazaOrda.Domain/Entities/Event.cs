using TazaOrda.Domain.Common;
using TazaOrda.Domain.Enums;

namespace TazaOrda.Domain.Entities;

/// <summary>
/// Акция, субботник или кампания, организованная компанией или администрацией
/// </summary>
public class Event : BaseEntity
{
    /// <summary>
    /// Название акции/события
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Описание акции
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Дата и время начала акции
    /// </summary>
    public DateTime StartAt { get; set; }

    /// <summary>
    /// Дата и время окончания акции
    /// </summary>
    public DateTime EndAt { get; set; }

    /// <summary>
    /// ID района, к которому относится акция (опционально)
    /// </summary>
    public Guid? DistrictId { get; set; }

    /// <summary>
    /// Район (навигационное свойство)
    /// </summary>
    public District? District { get; set; }

    /// <summary>
    /// Широта места проведения (опционально)
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// Долгота места проведения (опционально)
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// Награда в coins за участие
    /// </summary>
    public decimal CoinReward { get; set; }

    /// <summary>
    /// URL обложки события
    /// </summary>
    public string? CoverUrl { get; set; }

    /// <summary>
    /// Признак активности акции
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Список участников акции
    /// </summary>
    public ICollection<EventParticipant> Participants { get; set; } = new List<EventParticipant>();

    /// <summary>
    /// Количество зарегистрированных участников
    /// </summary>
    public int ParticipantsCount => Participants.Count(p => p.Status != EventParticipantStatus.Cancelled);

    /// <summary>
    /// Количество участников, которые завершили участие
    /// </summary>
    public int CompletedParticipantsCount => Participants.Count(p => p.Status == EventParticipantStatus.Completed);

    /// <summary>
    /// Проверка, началась ли акция
    /// </summary>
    public bool HasStarted => DateTime.UtcNow >= StartAt;

    /// <summary>
    /// Проверка, закончилась ли акция
    /// </summary>
    public bool HasEnded => DateTime.UtcNow >= EndAt;

    /// <summary>
    /// Проверка, идёт ли акция в данный момент
    /// </summary>
    public bool IsOngoing => HasStarted && !HasEnded;

    /// <summary>
    /// Проверка времени события (не в прошлом)
    /// </summary>
    public bool IsValidTime => StartAt > DateTime.UtcNow;

    /// <summary>
    /// Длительность акции
    /// </summary>
    public TimeSpan Duration => EndAt - StartAt;

    /// <summary>
    /// Присоединить участника к событию
    /// </summary>
    public EventParticipant JoinEvent(User user)
    {
        if (!IsActive)
            throw new InvalidOperationException("Событие неактивно");

        if (HasEnded)
            throw new InvalidOperationException("Событие уже завершено");

        // Проверить, не присоединился ли пользователь уже
        var existingParticipant = Participants.FirstOrDefault(p =>
            p.UserId == user.Id && p.Status != EventParticipantStatus.Cancelled);

        if (existingParticipant != null)
            throw new InvalidOperationException("Пользователь уже присоединился к этому событию");

        var participant = new EventParticipant
        {
            EventId = Id,
            Event = this,
            UserId = user.Id,
            User = user,
            Status = EventParticipantStatus.Joined,
            JoinedAt = DateTime.UtcNow
        };

        Participants.Add(participant);
        UpdatedAt = DateTime.UtcNow;

        return participant;
    }

    /// <summary>
    /// Деактивировать событие
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}