namespace TazaOrda.Domain.DTOs.Auth;

/// <summary>
/// DTO для регистрации пользователя
/// </summary>
public record RegisterRequest
{
    /// <summary>
    /// Номер телефона
    /// </summary>
    public string PhoneNumber { get; init; } = string.Empty;

    /// <summary>
    /// Пароль
    /// </summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Имя (опционально)
    /// </summary>
    public string? FirstName { get; init; }

    /// <summary>
    /// Фамилия (опционально)
    /// </summary>
    public string? LastName { get; init; }

    /// <summary>
    /// Email (опционально)
    /// </summary>
    public string? Email { get; init; }
}

/// <summary>
/// DTO для входа пользователя
/// </summary>
public record LoginRequest
{
    /// <summary>
    /// Номер телефона
    /// </summary>
    public string PhoneNumber { get; init; } = string.Empty;

    /// <summary>
    /// Пароль
    /// </summary>
    public string Password { get; init; } = string.Empty;
}

/// <summary>
/// DTO для обновления токена
/// </summary>
public record RefreshTokenRequest
{
    /// <summary>
    /// Refresh токен
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;
}

/// <summary>
/// Ответ на успешную аутентификацию
/// </summary>
public record AuthResponse
{
    /// <summary>
    /// JWT токен
    /// </summary>
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>
    /// Refresh токен
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;

    /// <summary>
    /// Тип токена
    /// </summary>
    public string TokenType { get; init; } = "Bearer";

    /// <summary>
    /// Время истечения токена (в секундах)
    /// </summary>
    public int ExpiresIn { get; init; }

    /// <summary>
    /// Информация о пользователе
    /// </summary>
    public UserInfo User { get; init; } = null!;
}

/// <summary>
/// Информация о пользователе
/// </summary>
public record UserInfo
{
    /// <summary>
    /// ID пользователя
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Имя
    /// </summary>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// Фамилия
    /// </summary>
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// Полное имя
    /// </summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>
    /// Номер телефона
    /// </summary>
    public string PhoneNumber { get; init; } = string.Empty;

    /// <summary>
    /// Email
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// Роль
    /// </summary>
    public string Role { get; init; } = string.Empty;

    /// <summary>
    /// Фото профиля
    /// </summary>
    public string? ProfilePhotoUrl { get; init; }

    /// <summary>
    /// Баланс монет
    /// </summary>
    public decimal CoinBalance { get; init; }

    /// <summary>
    /// Рейтинг активности
    /// </summary>
    public int ActivityRating { get; init; }
}
