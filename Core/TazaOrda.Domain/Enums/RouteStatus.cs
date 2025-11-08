namespace TazaOrda.Domain.Enums;

/// <summary>
/// Статусы маршрута
/// </summary>
public enum RouteStatus
{
    /// <summary>
    /// Планируется - маршрут создан, но ещё не начат
    /// </summary>
    Planned = 0,

    /// <summary>
    /// В пути - маршрут выполняется
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Выполнен - маршрут успешно завершён
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Отменён - маршрут отменён до выполнения
    /// </summary>
    Cancelled = 3,

    /// <summary>
    /// Приостановлен - временно остановлен
    /// </summary>
    Paused = 4
}