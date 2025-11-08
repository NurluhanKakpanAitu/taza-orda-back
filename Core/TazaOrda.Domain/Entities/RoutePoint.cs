using TazaOrda.Domain.Common;
using TazaOrda.Domain.ValueObjects;

namespace TazaOrda.Domain.Entities;

/// <summary>
/// Точка/контейнер на маршруте
/// </summary>
public class RoutePoint : BaseEntity
{
    /// <summary>
    /// ID маршрута, к которому относится точка
    /// </summary>
    public Guid RouteId { get; set; }

    /// <summary>
    /// Маршрут (навигационное свойство)
    /// </summary>
    public Route Route { get; set; } = null!;

    /// <summary>
    /// Порядковый номер точки на маршруте
    /// </summary>
    public int SequenceNumber { get; set; }

    /// <summary>
    /// Геолокация точки
    /// </summary>
    public Location Location { get; set; } = null!;

    /// <summary>
    /// Адрес точки
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Описание точки (например, "Контейнер у дома 15")
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Планируемое время посещения
    /// </summary>
    public DateTime? ScheduledTime { get; set; }

    /// <summary>
    /// Фактическое время посещения
    /// </summary>
    public DateTime? ActualTime { get; set; }

    /// <summary>
    /// Признак выполнения точки
    /// </summary>
    public bool IsCompleted { get; set; } = false;

    /// <summary>
    /// Комментарии по точке
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// URL фотографии (например, "до/после")
    /// </summary>
    public string? PhotoUrl { get; set; }

    /// <summary>
    /// Количество собранного мусора в точке (в кг)
    /// </summary>
    public double? CollectedWasteInKg { get; set; }

    /// <summary>
    /// Отметить точку как выполненную
    /// </summary>
    public void MarkAsCompleted(double? wasteCollected = null, string? notes = null, string? photoUrl = null)
    {
        IsCompleted = true;
        ActualTime = DateTime.UtcNow;
        CollectedWasteInKg = wasteCollected;
        Notes = notes;
        PhotoUrl = photoUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Проверка, пропущена ли точка (по времени)
    /// </summary>
    public bool IsOverdue
    {
        get
        {
            if (IsCompleted || ScheduledTime == null)
                return false;

            return DateTime.UtcNow > ScheduledTime.Value;
        }
    }
}