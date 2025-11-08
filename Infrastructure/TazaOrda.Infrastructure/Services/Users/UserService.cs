using Microsoft.EntityFrameworkCore;
using TazaOrda.Domain.DTOs.Users;
using TazaOrda.Domain.Interfaces;
using TazaOrda.Infrastructure.Persistence;

namespace TazaOrda.Infrastructure.Services.Users;

/// <summary>
/// Реализация сервиса для работы с профилем пользователя
/// </summary>
public class UserService(TazaOrdaDbContext context, IPasswordHasher passwordHasher) : IUserService
{
    public async Task<UserProfileDto?> GetUserProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await context.Users
            .Where(u => u.Id == userId)
            .Select(u => new UserProfileDto
            {
                Id = u.Id,
                Name = u.FullName,
                PhoneNumber = u.PhoneNumber,
                Email = u.Email,
                Role = u.Role.ToString(),
                Coins = u.CoinBalance,
                ReportsCount = u.ReportsCount,
                ActivityRating = u.ActivityRating,
                ProfilePhotoUrl = u.ProfilePhotoUrl,
                MemberSince = u.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        return user;
    }

    public async Task<UserProfileDto> UpdateUserProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await context.Users.FindAsync([userId], cancellationToken);

        if (user == null)
        {
            throw new InvalidOperationException("Пользователь не найден");
        }

        // Обновление имени
        if (!string.IsNullOrWhiteSpace(request.FirstName))
        {
            user.FirstName = request.FirstName;
        }

        if (!string.IsNullOrWhiteSpace(request.LastName))
        {
            user.LastName = request.LastName;
        }

        // Обновление email
        if (request.Email != null)
        {
            user.Email = request.Email;
        }

        // Обновление пароля
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            if (request.Password.Length < 6)
            {
                throw new InvalidOperationException("Пароль должен содержать минимум 6 символов");
            }
            user.PasswordHash = passwordHasher.HashPassword(request.Password);
        }

        user.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return new UserProfileDto
        {
            Id = user.Id,
            Name = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Email = user.Email,
            Role = user.Role.ToString(),
            Coins = user.CoinBalance,
            ReportsCount = user.ReportsCount,
            ActivityRating = user.ActivityRating,
            ProfilePhotoUrl = user.ProfilePhotoUrl,
            MemberSince = user.CreatedAt
        };
    }
}
