namespace TazaOrda.Domain.Enums;

/// <summary>
/// Типы действий для журнала аудита
/// </summary>
public enum AuditAction
{
    /// <summary>
    /// Создание новой записи
    /// </summary>
    Created = 0,

    /// <summary>
    /// Обновление существующей записи
    /// </summary>
    Updated = 1,

    /// <summary>
    /// Удаление записи
    /// </summary>
    Deleted = 2,

    /// <summary>
    /// Просмотр записи
    /// </summary>
    Viewed = 3,

    /// <summary>
    /// Вход в систему
    /// </summary>
    Login = 4,

    /// <summary>
    /// Выход из системы
    /// </summary>
    Logout = 5,

    /// <summary>
    /// Изменение статуса
    /// </summary>
    StatusChanged = 6,

    /// <summary>
    /// Назначение исполнителя
    /// </summary>
    Assigned = 7,

    /// <summary>
    /// Экспорт данных
    /// </summary>
    Exported = 8,

    /// <summary>
    /// Импорт данных
    /// </summary>
    Imported = 9,

    /// <summary>
    /// Другое действие
    /// </summary>
    Other = 99
}