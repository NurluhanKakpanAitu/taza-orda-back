using TazaOrda.Domain.Entities;

namespace TazaOrda.Domain.Interfaces;

/// <summary>
/// Интерфейс для генерации JWT токенов
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Генерация access токена
    /// </summary>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Генерация refresh токена
    /// </summary>
    RefreshToken GenerateRefreshToken(User user, string? ipAddress = null);

    /// <summary>
    /// Валидация токена
    /// </summary>
    Guid? ValidateToken(string token);

    /// <summary>
    /// Получить время истечения токена в секундах
    /// </summary>
    int GetTokenExpirationInSeconds();
}
