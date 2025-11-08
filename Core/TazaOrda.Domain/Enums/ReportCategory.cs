namespace TazaOrda.Domain.Enums;

/// <summary>
/// Категории обращений/заявок
/// </summary>
public enum ReportCategory
{
    /// <summary>
    /// Переполненный мусорный бак
    /// </summary>
    OverflowingBin = 0,

    /// <summary>
    /// Мусор на улице, в общественных местах
    /// </summary>
    StreetLitter = 1,

    /// <summary>
    /// Несанкционированная свалка
    /// </summary>
    IllegalDump = 2,

    /// <summary>
    /// Неубранный снег/лёд
    /// </summary>
    SnowIce = 3,

    /// <summary>
    /// Повреждённый контейнер
    /// </summary>
    DamagedContainer = 4,

    /// <summary>
    /// Нарушение графика вывоза мусора
    /// </summary>
    MissedCollection = 5,

    /// <summary>
    /// Загрязнение водоёмов
    /// </summary>
    WaterPollution = 6,

    /// <summary>
    /// Другое
    /// </summary>
    Other = 99
}