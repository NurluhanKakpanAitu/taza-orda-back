using Microsoft.EntityFrameworkCore;
using TazaOrda.Domain.DTOs.Auth;
using TazaOrda.Domain.Entities;
using TazaOrda.Domain.Enums;
using TazaOrda.Domain.Interfaces;
using TazaOrda.Infrastructure.Persistence;

namespace TazaOrda.Infrastructure.Services.Auth;

/// <summary>
/// Реализация сервиса аутентификации
/// </summary>
public class AuthService : IAuthService
{
    private readonly TazaOrdaDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(
        TazaOrdaDbContext context,
        IJwtTokenService jwtTokenService,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// Регистрация нового пользователя
    /// </summary>
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        // Валидация номера телефона
        if (string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            throw new InvalidOperationException("Номер телефона обязателен");
        }

        // Валидация пароля
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
        {
            throw new InvalidOperationException("Пароль должен содержать минимум 6 символов");
        }

        // Проверка, не зарегистрирован ли уже пользователь с таким номером
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber, cancellationToken);

        if (existingUser != null)
        {
            throw new InvalidOperationException("Пользователь с таким номером телефона уже зарегистрирован");
        }

        // Создание нового пользователя
        var user = new User
        {
            PhoneNumber = request.PhoneNumber,
            FirstName = request.FirstName ?? string.Empty,
            LastName = request.LastName ?? string.Empty,
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Role = UserRole.Resident,
            Status = UserStatus.Active,
            IsPhoneNumberVerified = true, // Автоматически подтверждаем при регистрации
            CoinBalance = 0,
            ActivityRating = 0,
            LastActivityDate = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        // Генерация токенов
        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken(user, ipAddress);

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        return CreateAuthResponse(user, accessToken, refreshToken.Token);
    }

    /// <summary>
    /// Вход пользователя
    /// </summary>
    public async Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        // Поиск пользователя по номеру телефона
        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber, cancellationToken);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Неверный номер телефона или пароль");
        }

        // Проверка пароля
        if (string.IsNullOrEmpty(user.PasswordHash) || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Неверный номер телефона или пароль");
        }

        // Проверка статуса пользователя
        if (user.Status != UserStatus.Active)
        {
            throw new UnauthorizedAccessException("Аккаунт заблокирован или неактивен");
        }

        // Обновление последней активности
        user.LastActivityDate = DateTime.UtcNow;

        // Генерация токенов
        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken(user, ipAddress);

        // Удаление старых неактивных токенов
        var expiredTokens = user.RefreshTokens
            .Where(t => !t.IsActive)
            .ToList();
        
        foreach (var token in expiredTokens)
        {
            _context.RefreshTokens.Remove(token);
        }

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        return CreateAuthResponse(user, accessToken, refreshToken.Token);
    }

    /// <summary>
    /// Обновление токена
    /// </summary>
    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var refreshToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (refreshToken == null || !refreshToken.IsActive)
        {
            throw new UnauthorizedAccessException("Недействительный refresh токен");
        }

        // Создание нового refresh токена и отзыв старого
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken(refreshToken.User, ipAddress);
        
        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.RevokedByIp = ipAddress;
        refreshToken.ReplacedByToken = newRefreshToken.Token;

        _context.RefreshTokens.Add(newRefreshToken);
        
        // Обновление последней активности
        refreshToken.User.LastActivityDate = DateTime.UtcNow;
        
        await _context.SaveChangesAsync(cancellationToken);

        // Генерация нового access токена
        var accessToken = _jwtTokenService.GenerateAccessToken(refreshToken.User);

        return CreateAuthResponse(refreshToken.User, accessToken, newRefreshToken.Token);
    }

    /// <summary>
    /// Выход пользователя (отзыв токена)
    /// </summary>
    public async Task RevokeTokenAsync(string token, string? ipAddress = null, CancellationToken cancellationToken = default)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);

        if (refreshToken == null || !refreshToken.IsActive)
        {
            throw new InvalidOperationException("Токен не найден или уже отозван");
        }

        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.RevokedByIp = ipAddress;

        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Получить пользователя по ID
    /// </summary>
    public async Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    /// <summary>
    /// Создание ответа аутентификации
    /// </summary>
    private AuthResponse CreateAuthResponse(User user, string accessToken, string refreshToken)
    {
        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            TokenType = "Bearer",
            ExpiresIn = _jwtTokenService.GetTokenExpirationInSeconds(),
            User = new UserInfo
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                Role = user.Role.ToString(),
                ProfilePhotoUrl = user.ProfilePhotoUrl,
                CoinBalance = user.CoinBalance,
                ActivityRating = user.ActivityRating
            }
        };
    }
}
