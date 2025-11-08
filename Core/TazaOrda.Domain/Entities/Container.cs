using TazaOrda.Domain.Common;
using TazaOrda.Domain.Enums;
using TazaOrda.Domain.ValueObjects;

namespace TazaOrda.Domain.Entities;

/// <summary>
/// Контейнер/площадка для сбора мусора
/// </summary>
public class Container : BaseEntity
{
    /// <summary>
    /// Адрес расположения контейнера
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Геолокация контейнера
    /// </summary>
    public Location Location { get; set; } = null!;

    /// <summary>
    /// Тип контейнера (бытовой, пластик, стекло и т.д.)
    /// </summary>
    public ContainerType Type { get; set; }

    /// <summary>
    /// Уровень заполненности в процентах (0-100), если есть IoT датчик
    /// </summary>
    public int? FillLevel { get; set; }

    /// <summary>
    /// Дата последнего опорожнения
    /// </summary>
    public DateTime? LastEmptiedAt { get; set; }

    /// <summary>
    /// ID района, к которому относится контейнер
    /// </summary>
    public Guid DistrictId { get; set; }

    /// <summary>
    /// Район (навигационное свойство)
    /// </summary>
    public District District { get; set; } = null!;

    /// <summary>
    /// QR-код контейнера для идентификации
    /// </summary>
    public string? QrCode { get; set; }

    /// <summary>
    /// ID IoT датчика (если установлен)
    /// </summary>
    public string? IoTSensorId { get; set; }

    /// <summary>
    /// Объём контейнера в литрах
    /// </summary>
    public int? CapacityInLiters { get; set; }

    /// <summary>
    /// Признак активности контейнера
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Состояние контейнера (например: "Исправен", "Требует ремонта", "Повреждён")
    /// </summary>
    public string? Condition { get; set; }

    /// <summary>
    /// Дата последнего обновления уровня заполненности
    /// </summary>
    public DateTime? LastFillLevelUpdate { get; set; }

    /// <summary>
    /// Описание/примечания о контейнере
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Дата установки контейнера
    /// </summary>
    public DateTime? InstalledAt { get; set; }

    /// <summary>
    /// Планируемая частота опорожнения (в днях)
    /// </summary>
    public int? ScheduledEmptyingFrequencyInDays { get; set; }

    /// <summary>
    /// Номер контейнера (инвентарный)
    /// </summary>
    public string? InventoryNumber { get; set; }

    /// <summary>
    /// Коллекция обращений, связанных с этим контейнером
    /// </summary>
    public ICollection<Report> Reports { get; set; } = new List<Report>();

    /// <summary>
    /// Проверка, есть ли IoT датчик
    /// </summary>
    public bool HasIoTSensor => !string.IsNullOrWhiteSpace(IoTSensorId);

    /// <summary>
    /// Проверка, переполнен ли контейнер (более 80%)
    /// </summary>
    public bool IsOverfilled => FillLevel.HasValue && FillLevel.Value >= 80;

    /// <summary>
    /// Проверка, требуется ли опорожнение
    /// </summary>
    public bool RequiresEmptying
    {
        get
        {
            // Если переполнен
            if (IsOverfilled)
                return true;

            // Если нет датчика, проверяем по расписанию
            if (!HasIoTSensor && LastEmptiedAt.HasValue && ScheduledEmptyingFrequencyInDays.HasValue)
            {
                var daysSinceLastEmptying = (DateTime.UtcNow - LastEmptiedAt.Value).Days;
                return daysSinceLastEmptying >= ScheduledEmptyingFrequencyInDays.Value;
            }

            return false;
        }
    }

    /// <summary>
    /// Количество дней с последнего опорожнения
    /// </summary>
    public int? DaysSinceLastEmptying
    {
        get
        {
            if (!LastEmptiedAt.HasValue)
                return null;

            return (DateTime.UtcNow - LastEmptiedAt.Value).Days;
        }
    }

    /// <summary>
    /// Проверка, просрочено ли опорожнение по расписанию
    /// </summary>
    public bool IsOverdue
    {
        get
        {
            if (!ScheduledEmptyingFrequencyInDays.HasValue || !LastEmptiedAt.HasValue)
                return false;

            var daysSinceEmptying = DaysSinceLastEmptying ?? 0;
            return daysSinceEmptying > ScheduledEmptyingFrequencyInDays.Value;
        }
    }

    /// <summary>
    /// Обновить уровень заполненности (от IoT датчика)
    /// </summary>
    public void UpdateFillLevel(int fillLevel)
    {
        if (fillLevel < 0 || fillLevel > 100)
        {
            throw new ArgumentException("Уровень заполненности должен быть в диапазоне от 0 до 100", nameof(fillLevel));
        }

        FillLevel = fillLevel;
        LastFillLevelUpdate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Опорожнить контейнер
    /// </summary>
    public void Empty()
    {
        LastEmptiedAt = DateTime.UtcNow;
        FillLevel = 0;
        LastFillLevelUpdate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Отметить контейнер как требующий ремонта
    /// </summary>
    public void MarkForRepair(string reason)
    {
        Condition = $"Требует ремонта: {reason}";
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Отметить контейнер как исправный после ремонта
    /// </summary>
    public void MarkAsRepaired()
    {
        Condition = "Исправен";
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Деактивировать контейнер
    /// </summary>
    public void Deactivate(string? reason = null)
    {
        IsActive = false;
        if (!string.IsNullOrWhiteSpace(reason))
        {
            Notes = $"{Notes}\n[{DateTime.UtcNow:yyyy-MM-dd}] Деактивирован: {reason}".Trim();
        }
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Активировать контейнер
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Назначить IoT датчик контейнеру
    /// </summary>
    public void AssignIoTSensor(string sensorId)
    {
        if (string.IsNullOrWhiteSpace(sensorId))
        {
            throw new ArgumentException("ID датчика не может быть пустым", nameof(sensorId));
        }

        IoTSensorId = sensorId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Удалить IoT датчик
    /// </summary>
    public void RemoveIoTSensor()
    {
        IoTSensorId = null;
        FillLevel = null;
        LastFillLevelUpdate = null;
        UpdatedAt = DateTime.UtcNow;
    }
}