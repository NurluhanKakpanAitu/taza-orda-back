namespace TazaOrda.Domain.Enums;

/// <summary>
/// Статусы транспортного средства
/// </summary>
public enum VehicleStatus
{
    /// <summary>
    /// Активен - в рабочем состоянии, готов к использованию
    /// </summary>
    Active = 0,

    /// <summary>
    /// На маршруте - выполняет задание
    /// </summary>
    OnRoute = 1,

    /// <summary>
    /// На ремонте - временно не работает
    /// </summary>
    InRepair = 2,

    /// <summary>
    /// Неисправен - требует ремонта
    /// </summary>
    OutOfOrder = 3,

    /// <summary>
    /// В простое - готов к работе, но не используется
    /// </summary>
    Idle = 4,

    /// <summary>
    /// Списан - выведен из эксплуатации
    /// </summary>
    Retired = 5
}