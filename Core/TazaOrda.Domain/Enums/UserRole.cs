namespace TazaOrda.Domain.Enums;

/// <summary>
/// Роли пользователей в системе
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Житель города - обычный пользователь, который может отправлять жалобы и участвовать в программах
    /// </summary>
    Resident = 0,

    /// <summary>
    /// Оператор - сотрудник компании, который обрабатывает запросы и жалобы
    /// </summary>
    Operator = 1,

    /// <summary>
    /// выполняет уборку, прикладывает фото «до/после»
    /// </summary>
    Inspector = 2,

    /// <summary>
    /// Администратор - полный доступ к системе
    /// </summary>
    Admin = 3
}