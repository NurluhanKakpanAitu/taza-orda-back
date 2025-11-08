using TazaOrda.Domain.Common;
using TazaOrda.Domain.Enums;

namespace TazaOrda.Domain.Entities;

/// <summary>
/// Участие пользователя в акции/мероприятии
/// </summary>
public class EventParticipant : BaseEntity
{
    /// <summary>
    /// ID пользователя
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Пользователь (навигационное свойство)
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// ID акции
    /// </summary>
    public Guid EventId { get; set; }

    /// <summary>
    /// Акция (навигационное свойство)
    /// </summary>
    public Event Event { get; set; } = null!;

    /// <summary>
    /// Статус участия
    /// </summary>
    public EventParticipantStatus Status { get; set; } = EventParticipantStatus.Joined;

    /// <summary>
    /// Дата присоединения к событию
    /// </summary>
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Дата чек-ина
    /// </summary>
    public DateTime? CheckedInAt { get; set; }

    /// <summary>
    /// Дата начисления монет (завершения участия)
    /// </summary>
    public DateTime? CoinsAwardedAt { get; set; }

    /// <summary>
    /// Количество начисленных coins за участие
    /// </summary>
    public decimal CoinsAwarded { get; set; } = 0;

    /// <summary>
    /// ID транзакции с начислением coins
    /// </summary>
    public Guid? CoinTransactionId { get; set; }

    /// <summary>
    /// Транзакция с начислением coins (навигационное свойство)
    /// </summary>
    public CoinTransaction? CoinTransaction { get; set; }

    /// <summary>
    /// Проверка, выполнен ли чек-ин
    /// </summary>
    public bool IsCheckedIn => Status == EventParticipantStatus.CheckedIn || Status == EventParticipantStatus.Completed;

    /// <summary>
    /// Проверка, завершено ли участие
    /// </summary>
    public bool IsCompleted => Status == EventParticipantStatus.Completed;

    /// <summary>
    /// Проверка, отменено ли участие
    /// </summary>
    public bool IsCancelled => Status == EventParticipantStatus.Cancelled;

    /// <summary>
    /// Выполнить чек-ин
    /// </summary>
    public void CheckIn()
    {
        if (Status == EventParticipantStatus.Cancelled)
            throw new InvalidOperationException("Нельзя выполнить чек-ин для отменённой регистрации");

        if (Status == EventParticipantStatus.Completed)
            throw new InvalidOperationException("Участие уже завершено");

        Status = EventParticipantStatus.CheckedIn;
        CheckedInAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Завершить участие и начислить монеты
    /// </summary>
    public void Complete(decimal coinReward, CoinTransaction? transaction = null)
    {
        if (Status == EventParticipantStatus.Cancelled)
            throw new InvalidOperationException("Нельзя завершить отменённую регистрацию");

        if (Status == EventParticipantStatus.Completed)
            throw new InvalidOperationException("Участие уже завершено");

        Status = EventParticipantStatus.Completed;
        CoinsAwarded = coinReward;
        CoinsAwardedAt = DateTime.UtcNow;

        if (transaction != null)
        {
            CoinTransactionId = transaction.Id;
            CoinTransaction = transaction;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Отменить участие
    /// </summary>
    public void Cancel()
    {
        if (Status == EventParticipantStatus.Completed)
            throw new InvalidOperationException("Нельзя отменить завершённое участие");

        Status = EventParticipantStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Связать транзакцию с начислением coins
    /// </summary>
    public void LinkCoinTransaction(CoinTransaction transaction)
    {
        CoinTransactionId = transaction.Id;
        CoinTransaction = transaction;
        UpdatedAt = DateTime.UtcNow;
    }
}