namespace TazaOrda.Domain.Enums;

/// <summary>
/// Статусы пользователя в системе
/// </summary>
public enum UserStatus
{
    /// <summary>
    /// Активный пользователь
    /// </summary>
    Active = 0,

    /// <summary>
    /// Заблокированный пользователь
    /// </summary>
    Blocked = 1,

    /// <summary>
    /// Неактивный пользователь (временно)
    /// </summary>
    Inactive = 2
}