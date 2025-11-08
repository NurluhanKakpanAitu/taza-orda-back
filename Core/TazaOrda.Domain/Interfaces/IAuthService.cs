using TazaOrda.Domain.DTOs.Auth;
using TazaOrda.Domain.Entities;

namespace TazaOrda.Domain.Interfaces;

/// <summary>
/// Интерфейс для сервиса аутентификации
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Регистрация нового пользователя
    /// </summary>
    Task<AuthResponse> RegisterAsync(RegisterRequest request, string? ipAddress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Вход пользователя
    /// </summary>
    Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновление токена
    /// </summary>
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Выход пользователя (отзыв токена)
    /// </summary>
    Task RevokeTokenAsync(string token, string? ipAddress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить пользователя по ID
    /// </summary>
    Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
