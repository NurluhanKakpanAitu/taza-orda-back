namespace TazaOrda.Domain.Enums;

/// <summary>
/// Типы контейнеров для мусора
/// </summary>
public enum ContainerType
{
    /// <summary>
    /// Бытовой мусор (смешанные отходы)
    /// </summary>
    General = 0,

    /// <summary>
    /// Пластик (раздельный сбор)
    /// </summary>
    Plastic = 1,

    /// <summary>
    /// Стекло (раздельный сбор)
    /// </summary>
    Glass = 2,

    /// <summary>
    /// Бумага и картон (раздельный сбор)
    /// </summary>
    Paper = 3,

    /// <summary>
    /// Металл (раздельный сбор)
    /// </summary>
    Metal = 4,

    /// <summary>
    /// Органические отходы
    /// </summary>
    Organic = 5,

    /// <summary>
    /// Крупногабаритный мусор
    /// </summary>
    Bulky = 6,

    /// <summary>
    /// Опасные отходы (батарейки, лампы и т.д.)
    /// </summary>
    Hazardous = 7,

    /// <summary>
    /// Строительный мусор
    /// </summary>
    Construction = 8
}