using TazaOrda.Domain.Common;

namespace TazaOrda.Domain.Entities;

/// <summary>
/// Награда/подарок/бонус, на который можно обменять coins
/// </summary>
public class Reward : BaseEntity
{
    /// <summary>
    /// Название награды
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Описание награды
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Количество coins для обмена
    /// </summary>
    public decimal CoinsCost { get; set; }

    /// <summary>
    /// Поставщик/партнёр
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// Категория награды (например: "Скидки", "Товары", "Услуги", "Сертификаты")
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Изображение награды
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Общее количество доступных наград (null = неограниченно)
    /// </summary>
    public int? TotalQuantity { get; set; }

    /// <summary>
    /// Количество уже выданных наград
    /// </summary>
    public int RedeemedQuantity { get; set; } = 0;

    /// <summary>
    /// Признак доступности награды
    /// </summary>
    public bool IsAvailable { get; set; } = true;

    /// <summary>
    /// Дата начала действия награды
    /// </summary>
    public DateTime? ValidFrom { get; set; }

    /// <summary>
    /// Дата окончания действия награды
    /// </summary>
    public DateTime? ValidUntil { get; set; }

    /// <summary>
    /// Коллекция обменов этой награды
    /// </summary>
    public ICollection<RewardRedemption> Redemptions { get; set; } = new List<RewardRedemption>();

    /// <summary>
    /// Условия получения награды
    /// </summary>
    public string? Terms { get; set; }

    /// <summary>
    /// Инструкции по получению награды
    /// </summary>
    public string? RedemptionInstructions { get; set; }

    /// <summary>
    /// Контактная информация поставщика
    /// </summary>
    public string? ProviderContact { get; set; }

    /// <summary>
    /// Минимальный уровень пользователя для обмена (если применимо)
    /// </summary>
    public int? MinimumUserLevel { get; set; }

    /// <summary>
    /// Приоритет отображения (чем больше, тем выше)
    /// </summary>
    public int DisplayPriority { get; set; } = 0;

    /// <summary>
    /// Количество оставшихся наград
    /// </summary>
    public int? RemainingQuantity
    {
        get
        {
            if (!TotalQuantity.HasValue)
                return null;

            return Math.Max(0, TotalQuantity.Value - RedeemedQuantity);
        }
    }

    /// <summary>
    /// Проверка, закончились ли награды
    /// </summary>
    public bool IsOutOfStock
    {
        get
        {
            if (!TotalQuantity.HasValue)
                return false;

            return RedeemedQuantity >= TotalQuantity.Value;
        }
    }

    /// <summary>
    /// Проверка, доступна ли награда для обмена в данный момент
    /// </summary>
    public bool IsCurrentlyAvailable
    {
        get
        {
            if (!IsAvailable || IsOutOfStock)
                return false;

            var now = DateTime.UtcNow;

            if (ValidFrom.HasValue && now < ValidFrom.Value)
                return false;

            if (ValidUntil.HasValue && now > ValidUntil.Value)
                return false;

            return true;
        }
    }

    /// <summary>
    /// Проверка, истёк ли срок действия награды
    /// </summary>
    public bool IsExpired
    {
        get
        {
            if (!ValidUntil.HasValue)
                return false;

            return DateTime.UtcNow > ValidUntil.Value;
        }
    }

    /// <summary>
    /// Проверка, может ли пользователь обменять эту награду
    /// </summary>
    public bool CanBeRedeemedBy(User user)
    {
        if (!IsCurrentlyAvailable)
            return false;

        if (user.CoinBalance < CoinsCost)
            return false;

        if (MinimumUserLevel.HasValue && user.ActivityRating < MinimumUserLevel.Value)
            return false;

        return true;
    }

    /// <summary>
    /// Обменять награду (уменьшить количество доступных)
    /// </summary>
    public RewardRedemption Redeem(User user)
    {
        if (!CanBeRedeemedBy(user))
        {
            throw new InvalidOperationException($"Пользователь не может обменять награду '{Name}'");
        }

        if (IsOutOfStock)
        {
            throw new InvalidOperationException($"Награда '{Name}' закончилась");
        }

        RedeemedQuantity++;
        UpdatedAt = DateTime.UtcNow;

        return new RewardRedemption
        {
            RewardId = Id,
            Reward = this,
            UserId = user.Id,
            User = user,
            CoinsSpent = CoinsCost,
            RedeemedAt = DateTime.UtcNow,
            Status = "Pending"
        };
    }

    /// <summary>
    /// Увеличить количество доступных наград
    /// </summary>
    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Количество должно быть положительным", nameof(quantity));
        }

        if (TotalQuantity.HasValue)
        {
            TotalQuantity += quantity;
        }
        else
        {
            TotalQuantity = quantity;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Изменить стоимость награды
    /// </summary>
    public void UpdateCost(decimal newCost)
    {
        if (newCost < 0)
        {
            throw new ArgumentException("Стоимость не может быть отрицательной", nameof(newCost));
        }

        CoinsCost = newCost;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Деактивировать награду
    /// </summary>
    public void Deactivate()
    {
        IsAvailable = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Активировать награду
    /// </summary>
    public void Activate()
    {
        IsAvailable = true;
        UpdatedAt = DateTime.UtcNow;
    }
}