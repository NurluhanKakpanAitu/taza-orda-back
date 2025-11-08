using TazaOrda.Domain.DTOs.Users;
using TazaOrda.Domain.Entities;

namespace TazaOrda.Domain.Interfaces;

/// <summary>
/// Интерфейс сервиса для работы с профилем пользователя
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Получить профиль пользователя
    /// </summary>
    Task<UserProfileDto?> GetUserProfileAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновить профиль пользователя
    /// </summary>
    Task<UserProfileDto> UpdateUserProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
}
