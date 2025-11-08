namespace TazaOrda.Domain.DTOs.Users;

/// <summary>
/// DTO профиля пользователя
/// </summary>
public record UserProfileDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string Role { get; init; } = string.Empty;
    public decimal Coins { get; init; }
    public int ReportsCount { get; init; }
    public int ActivityRating { get; init; }
    public string? ProfilePhotoUrl { get; init; }
    public DateTime MemberSince { get; init; }
}

/// <summary>
/// DTO для обновления профиля
/// </summary>
public record UpdateProfileRequest
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Email { get; init; }
    public string? Password { get; init; }
}
