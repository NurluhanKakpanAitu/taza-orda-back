using TazaOrda.Domain.Common;
using TazaOrda.Domain.Enums;

namespace TazaOrda.Domain.Entities;

/// <summary>
/// Транзакция с внутренней валютой (coins) - движение баллов пользователя
/// </summary>
public class CoinTransaction : BaseEntity
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
    /// Тип транзакции (начисление или списание)
    /// </summary>
    public CoinTransactionType Type { get; set; }

    /// <summary>
    /// Причина транзакции
    /// </summary>
    public CoinTransactionReason Reason { get; set; }

    /// <summary>
    /// Количество coins (всегда положительное число)
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Баланс пользователя после транзакции
    /// </summary>
    public decimal BalanceAfter { get; set; }

    /// <summary>
    /// Описание транзакции
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// ID связанного обращения (если применимо)
    /// </summary>
    public Guid? RelatedReportId { get; set; }

    /// <summary>
    /// Связанное обращение (навигационное свойство)
    /// </summary>
    public Report? RelatedReport { get; set; }

    /// <summary>
    /// ID связанной акции (если применимо)
    /// </summary>
    public Guid? RelatedEventId { get; set; }

    /// <summary>
    /// Связанная акция (навигационное свойство)
    /// </summary>
    public Event? RelatedEvent { get; set; }

    /// <summary>
    /// ID связанной награды (если применимо)
    /// </summary>
    public Guid? RelatedRewardId { get; set; }

    /// <summary>
    /// Связанная награда (навигационное свойство)
    /// </summary>
    public Reward? RelatedReward { get; set; }

    /// <summary>
    /// ID администратора, который выполнил операцию (если применимо)
    /// </summary>
    public Guid? ProcessedByAdminId { get; set; }

    /// <summary>
    /// Администратор, выполнивший операцию (навигационное свойство)
    /// </summary>
    public User? ProcessedByAdmin { get; set; }

    /// <summary>
    /// Признак отмены транзакции
    /// </summary>
    public bool IsReversed { get; set; } = false;

    /// <summary>
    /// ID транзакции, которая отменила эту (если применимо)
    /// </summary>
    public Guid? ReversalTransactionId { get; set; }

    /// <summary>
    /// Дата обработки транзакции
    /// </summary>
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Проверка, является ли транзакция начислением
    /// </summary>
    public bool IsCredit => Type == CoinTransactionType.Credit;

    /// <summary>
    /// Проверка, является ли транзакция списанием
    /// </summary>
    public bool IsDebit => Type == CoinTransactionType.Debit;

    /// <summary>
    /// Сумма с учётом типа (положительная для начисления, отрицательная для списания)
    /// </summary>
    public decimal SignedAmount => Type == CoinTransactionType.Credit ? Amount : -Amount;

    /// <summary>
    /// Создать транзакцию начисления
    /// </summary>
    public static CoinTransaction CreateCredit(
        User user,
        decimal amount,
        CoinTransactionReason reason,
        string? description = null)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Сумма начисления должна быть положительной", nameof(amount));
        }

        var newBalance = user.CoinBalance + amount;

        return new CoinTransaction
        {
            UserId = user.Id,
            User = user,
            Type = CoinTransactionType.Credit,
            Reason = reason,
            Amount = amount,
            BalanceAfter = newBalance,
            Description = description,
            ProcessedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Создать транзакцию списания
    /// </summary>
    public static CoinTransaction CreateDebit(
        User user,
        decimal amount,
        CoinTransactionReason reason,
        string? description = null)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Сумма списания должна быть положительной", nameof(amount));
        }

        if (user.CoinBalance < amount)
        {
            throw new InvalidOperationException($"Недостаточно средств. Баланс: {user.CoinBalance}, требуется: {amount}");
        }

        var newBalance = user.CoinBalance - amount;

        return new CoinTransaction
        {
            UserId = user.Id,
            User = user,
            Type = CoinTransactionType.Debit,
            Reason = reason,
            Amount = amount,
            BalanceAfter = newBalance,
            Description = description,
            ProcessedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Отменить транзакцию (создать обратную)
    /// </summary>
    public CoinTransaction Reverse(string reason, User? admin = null)
    {
        if (IsReversed)
        {
            throw new InvalidOperationException("Транзакция уже отменена");
        }

        IsReversed = true;
        UpdatedAt = DateTime.UtcNow;

        var reversalType = Type == CoinTransactionType.Credit
            ? CoinTransactionType.Debit
            : CoinTransactionType.Credit;

        return new CoinTransaction
        {
            UserId = UserId,
            User = User,
            Type = reversalType,
            Reason = CoinTransactionReason.Reversal,
            Amount = Amount,
            BalanceAfter = Type == CoinTransactionType.Credit
                ? User.CoinBalance - Amount
                : User.CoinBalance + Amount,
            Description = $"Отмена транзакции {Id}: {reason}",
            ProcessedByAdminId = admin?.Id,
            ProcessedByAdmin = admin,
            ReversalTransactionId = Id,
            ProcessedAt = DateTime.UtcNow
        };
    }
}