using TazaOrda.Domain.Common;

namespace TazaOrda.Domain.Entities;

/// <summary>
/// Обмен награды пользователем (история получения наград)
/// </summary>
public class RewardRedemption : BaseEntity
{
    /// <summary>
    /// ID награды
    /// </summary>
    public Guid RewardId { get; set; }

    /// <summary>
    /// Награда (навигационное свойство)
    /// </summary>
    public Reward Reward { get; set; } = null!;

    /// <summary>
    /// ID пользователя
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Пользователь (навигационное свойство)
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Количество потраченных coins
    /// </summary>
    public decimal CoinsSpent { get; set; }

    /// <summary>
    /// ID транзакции со списанием coins
    /// </summary>
    public Guid? CoinTransactionId { get; set; }

    /// <summary>
    /// Транзакция со списанием coins (навигационное свойство)
    /// </summary>
    public CoinTransaction? CoinTransaction { get; set; }

    /// <summary>
    /// Дата обмена
    /// </summary>
    public DateTime RedeemedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Статус обмена (например: "Pending", "Approved", "Delivered", "Cancelled")
    /// </summary>
    public string Status { get; set; } = "Pending";

    /// <summary>
    /// Код подтверждения/промокод для получения награды
    /// </summary>
    public string? RedemptionCode { get; set; }

    /// <summary>
    /// Дата использования/получения награды
    /// </summary>
    public DateTime? UsedAt { get; set; }

    /// <summary>
    /// Дата истечения срока действия (если применимо)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Примечания от пользователя
    /// </summary>
    public string? UserNotes { get; set; }

    /// <summary>
    /// Примечания от администратора
    /// </summary>
    public string? AdminNotes { get; set; }

    /// <summary>
    /// ID администратора, обработавшего обмен
    /// </summary>
    public Guid? ProcessedByAdminId { get; set; }

    /// <summary>
    /// Администратор, обработавший обмен (навигационное свойство)
    /// </summary>
    public User? ProcessedByAdmin { get; set; }

    /// <summary>
    /// Дата обработки администратором
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Проверка, ожидает ли обмен обработки
    /// </summary>
    public bool IsPending => Status == "Pending";

    /// <summary>
    /// Проверка, одобрен ли обмен
    /// </summary>
    public bool IsApproved => Status == "Approved" || Status == "Delivered";

    /// <summary>
    /// Проверка, отменён ли обмен
    /// </summary>
    public bool IsCancelled => Status == "Cancelled";

    /// <summary>
    /// Проверка, получена ли награда
    /// </summary>
    public bool IsDelivered => Status == "Delivered";

    /// <summary>
    /// Проверка, истёк ли срок действия
    /// </summary>
    public bool IsExpired
    {
        get
        {
            if (!ExpiresAt.HasValue)
                return false;

            return DateTime.UtcNow > ExpiresAt.Value && !IsDelivered;
        }
    }

    /// <summary>
    /// Одобрить обмен
    /// </summary>
    public void Approve(User admin, string? redemptionCode = null, DateTime? expiresAt = null)
    {
        if (!IsPending)
        {
            throw new InvalidOperationException("Можно одобрить только ожидающий обмен");
        }

        Status = "Approved";
        ProcessedByAdminId = admin.Id;
        ProcessedByAdmin = admin;
        ProcessedAt = DateTime.UtcNow;
        RedemptionCode = redemptionCode ?? Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
        ExpiresAt = expiresAt;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Отметить награду как полученную/использованную
    /// </summary>
    public void MarkAsDelivered(string? notes = null)
    {
        if (IsCancelled)
        {
            throw new InvalidOperationException("Нельзя отметить отменённый обмен как доставленный");
        }

        Status = "Delivered";
        UsedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(notes))
        {
            AdminNotes = notes;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Отменить обмен
    /// </summary>
    public void Cancel(string reason, User? admin = null)
    {
        if (IsDelivered)
        {
            throw new InvalidOperationException("Нельзя отменить уже доставленную награду");
        }

        Status = "Cancelled";
        AdminNotes = $"Отменено: {reason}";
        if (admin != null)
        {
            ProcessedByAdminId = admin.Id;
            ProcessedByAdmin = admin;
            ProcessedAt = DateTime.UtcNow;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Связать транзакцию со списанием coins
    /// </summary>
    public void LinkCoinTransaction(CoinTransaction transaction)
    {
        CoinTransactionId = transaction.Id;
        CoinTransaction = transaction;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Установить код подтверждения
    /// </summary>
    public void SetRedemptionCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Код не может быть пустым", nameof(code));
        }

        RedemptionCode = code;
        UpdatedAt = DateTime.UtcNow;
    }
}