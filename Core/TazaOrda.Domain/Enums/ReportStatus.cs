namespace TazaOrda.Domain.Enums;

/// <summary>
/// Статусы обращения/заявки
/// </summary>
public enum ReportStatus
{
    /// <summary>
    /// Новое обращение, ожидает рассмотрения
    /// </summary>
    New = 0,

    /// <summary>
    /// В работе - назначен исполнитель, работа ведётся
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Выполнено - проблема решена
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Отклонено - заявка признана недействительной или дубликатом
    /// </summary>
    Rejected = 3,

    /// <summary>
    /// На проверке - работа выполнена, ожидает подтверждения
    /// </summary>
    UnderReview = 4,

    /// <summary>
    /// Закрыто - обращение закрыто с подтверждением
    /// </summary>
    Closed = 5
}