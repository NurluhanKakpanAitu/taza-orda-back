using TazaOrda.Domain.Common;

namespace TazaOrda.Domain.Entities;

/// <summary>
/// Refresh токен для обновления JWT
/// </summary>
public class RefreshToken : BaseEntity
{
    /// <summary>
    /// ID пользователя
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Пользователь
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Токен
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Дата истечения
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Дата отзыва (если токен отозван)
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// IP адрес создания токена
    /// </summary>
    public string? CreatedByIp { get; set; }

    /// <summary>
    /// IP адрес отзыва токена
    /// </summary>
    public string? RevokedByIp { get; set; }

    /// <summary>
    /// Токен, который заменил данный токен
    /// </summary>
    public string? ReplacedByToken { get; set; }

    /// <summary>
    /// Проверка, активен ли токен
    /// </summary>
    public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;

    /// <summary>
    /// Проверка, истёк ли токен
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}
