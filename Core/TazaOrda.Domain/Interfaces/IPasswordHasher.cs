namespace TazaOrda.Domain.Interfaces;

/// <summary>
/// Интерфейс для хэширования паролей
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Хэширование пароля
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Проверка пароля
    /// </summary>
    bool VerifyPassword(string password, string passwordHash);
}
