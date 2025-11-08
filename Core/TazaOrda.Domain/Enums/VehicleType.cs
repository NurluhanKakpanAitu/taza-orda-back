namespace TazaOrda.Domain.Enums;

/// <summary>
/// Типы транспортных средств
/// </summary>
public enum VehicleType
{
    /// <summary>
    /// Мусоровоз - основной транспорт для вывоза мусора
    /// </summary>
    GarbageTruck = 0,

    /// <summary>
    /// Компактор - мусоровоз с прессом
    /// </summary>
    Compactor = 1,

    /// <summary>
    /// Трактор - для уборки снега и крупного мусора
    /// </summary>
    Tractor = 2,

    /// <summary>
    /// Погрузчик
    /// </summary>
    Loader = 3,

    /// <summary>
    /// Подметальная машина
    /// </summary>
    StreetSweeper = 4,

    /// <summary>
    /// Поливальная машина
    /// </summary>
    WaterTruck = 5,

    /// <summary>
    /// Другая спецтехника
    /// </summary>
    OtherEquipment = 99
}