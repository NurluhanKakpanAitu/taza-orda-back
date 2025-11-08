using TazaOrda.Domain.Common;
using TazaOrda.Domain.Enums;

namespace TazaOrda.Domain.Entities;

/// <summary>
/// Пользователь системы (житель города, волонтёр, сотрудник компании или администратор)
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// Имя пользователя
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Фамилия пользователя
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Номер телефона (верифицируется через Firebase)
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Email адрес (опционально)
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Роль пользователя в системе
    /// </summary>
    public UserRole Role { get; set; } = UserRole.Resident;

    /// <summary>
    /// URL фотографии профиля (опционально)
    /// </summary>
    public string? ProfilePhotoUrl { get; set; }

    /// <summary>
    /// Баланс внутренней валюты (coins) для системы поощрений
    /// </summary>
    public decimal CoinBalance { get; set; } = 0;

    /// <summary>
    /// Рейтинг / уровень активности пользователя
    /// </summary>
    public int ActivityRating { get; set; } = 0;

    /// <summary>
    /// Статус пользователя (активен / заблокирован / неактивен)
    /// </summary>
    public UserStatus Status { get; set; } = UserStatus.Active;

    /// <summary>
    /// Хэш пароля (bcrypt)
    /// </summary>
    public string? PasswordHash { get; set; }

    /// <summary>
    /// Флаг подтверждения номера телефона
    /// </summary>
    public bool IsPhoneNumberVerified { get; set; } = false;

    /// <summary>
    /// Дата последней активности
    /// </summary>
    public DateTime? LastActivityDate { get; set; }

    /// <summary>
    /// Количество отправленных жалоб/отчётов
    /// </summary>
    public int ReportsCount { get; set; } = 0;

    /// <summary>
    /// ID компании, к которой относится сотрудник (опционально)
    /// </summary>
    public Guid? CompanyId { get; set; }

    /// <summary>
    /// Компания, к которой относится сотрудник (навигационное свойство)
    /// </summary>
    public Company? Company { get; set; }

    /// <summary>
    /// Обращения, созданные пользователем
    /// </summary>
    public ICollection<Report> Reports { get; set; } = new List<Report>();

    /// <summary>
    /// Обращения, назначенные пользователю
    /// </summary>
    public ICollection<Report> AssignedReports { get; set; } = new List<Report>();

    /// <summary>
    /// Транзакции с coins пользователя
    /// </summary>
    public ICollection<CoinTransaction> CoinTransactions { get; set; } = new List<CoinTransaction>();

    /// <summary>
    /// Уведомления пользователя
    /// </summary>
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    /// <summary>
    /// Refresh токены пользователя
    /// </summary>
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    /// <summary>
    /// Полное имя пользователя
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Проверка, является ли пользователь сотрудником компании
    /// </summary>
    public bool IsEmployee => Role is UserRole.Operator or UserRole.Inspector or UserRole.Admin;

    /// <summary>
    /// Проверка, является ли пользователь администратором
    /// </summary>
    public bool IsAdmin => Role == UserRole.Admin;

    /// <summary>
    /// Проверка, активен ли пользователь
    /// </summary>
    public bool IsActive => Status == UserStatus.Active;
}