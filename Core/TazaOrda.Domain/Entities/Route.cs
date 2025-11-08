using TazaOrda.Domain.Common;
using TazaOrda.Domain.Enums;
using TazaOrda.Domain.ValueObjects;

namespace TazaOrda.Domain.Entities;

/// <summary>
/// Маршрут или задание для вывоза мусора
/// </summary>
public class Route : BaseEntity
{
    /// <summary>
    /// Название/код маршрута
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Дата и время начала маршрута
    /// </summary>
    public DateTime ScheduledStartTime { get; set; }

    /// <summary>
    /// Планируемое время завершения
    /// </summary>
    public DateTime? ScheduledEndTime { get; set; }

    /// <summary>
    /// Фактическое время начала
    /// </summary>
    public DateTime? ActualStartTime { get; set; }

    /// <summary>
    /// Фактическое время завершения
    /// </summary>
    public DateTime? ActualEndTime { get; set; }

    /// <summary>
    /// ID ответственной бригады
    /// </summary>
    public Guid TeamId { get; set; }

    /// <summary>
    /// Ответственная бригада (навигационное свойство)
    /// </summary>
    public Team Team { get; set; } = null!;

    /// <summary>
    /// ID транспортного средства
    /// </summary>
    public Guid? VehicleId { get; set; }

    /// <summary>
    /// Транспортное средство (навигационное свойство)
    /// </summary>
    public Vehicle? Vehicle { get; set; }

    /// <summary>
    /// Статус маршрута
    /// </summary>
    public RouteStatus Status { get; set; } = RouteStatus.Planned;

    /// <summary>
    /// Список точек/контейнеров на маршруте
    /// </summary>
    public ICollection<RoutePoint> RoutePoints { get; set; } = new List<RoutePoint>();

    /// <summary>
    /// Описание маршрута
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Общая дистанция маршрута в километрах
    /// </summary>
    public double? TotalDistanceInKm { get; set; }

    /// <summary>
    /// Планируемое количество собранного мусора в тоннах
    /// </summary>
    public double? EstimatedWasteInTons { get; set; }

    /// <summary>
    /// Фактическое количество собранного мусора в тоннах
    /// </summary>
    public double? ActualWasteInTons { get; set; }

    /// <summary>
    /// Комментарии по завершению маршрута
    /// </summary>
    public string? CompletionNotes { get; set; }

    /// <summary>
    /// ID района, к которому относится маршрут
    /// </summary>
    public Guid? DistrictId { get; set; }

    /// <summary>
    /// Район (навигационное свойство)
    /// </summary>
    public District? District { get; set; }

    /// <summary>
    /// Проверка, выполняется ли маршрут в данный момент
    /// </summary>
    public bool IsActive => Status == RouteStatus.InProgress;

    /// <summary>
    /// Проверка, завершён ли маршрут
    /// </summary>
    public bool IsCompleted => Status == RouteStatus.Completed;

    /// <summary>
    /// Проверка, просрочен ли маршрут
    /// </summary>
    public bool IsOverdue
    {
        get
        {
            if (Status == RouteStatus.Completed || ScheduledEndTime == null)
                return false;

            return DateTime.UtcNow > ScheduledEndTime.Value;
        }
    }

    /// <summary>
    /// Длительность выполнения маршрута
    /// </summary>
    public TimeSpan? Duration
    {
        get
        {
            if (ActualStartTime == null || ActualEndTime == null)
                return null;

            return ActualEndTime.Value - ActualStartTime.Value;
        }
    }

    /// <summary>
    /// Количество точек на маршруте
    /// </summary>
    public int PointsCount => RoutePoints.Count;

    /// <summary>
    /// Количество завершённых точек
    /// </summary>
    public int CompletedPointsCount => RoutePoints.Count(p => p.IsCompleted);

    /// <summary>
    /// Процент выполнения маршрута
    /// </summary>
    public double CompletionPercentage
    {
        get
        {
            if (PointsCount == 0)
                return 0;

            return (double)CompletedPointsCount / PointsCount * 100;
        }
    }

    /// <summary>
    /// Начать выполнение маршрута
    /// </summary>
    public void Start()
    {
        if (Status != RouteStatus.Planned)
        {
            throw new InvalidOperationException($"Маршрут {Name} не может быть начат. Текущий статус: {Status}");
        }

        Status = RouteStatus.InProgress;
        ActualStartTime = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // Обновить статус транспорта
        Vehicle?.StartRoute();
    }

    /// <summary>
    /// Завершить маршрут
    /// </summary>
    public void Complete(double? actualWaste = null, string? notes = null)
    {
        if (Status != RouteStatus.InProgress)
        {
            throw new InvalidOperationException($"Маршрут {Name} не может быть завершён. Текущий статус: {Status}");
        }

        Status = RouteStatus.Completed;
        ActualEndTime = DateTime.UtcNow;
        ActualWasteInTons = actualWaste;
        CompletionNotes = notes;
        UpdatedAt = DateTime.UtcNow;

        // Обновить статус транспорта
        Vehicle?.CompleteRoute();
    }

    /// <summary>
    /// Отменить маршрут
    /// </summary>
    public void Cancel(string reason)
    {
        if (Status == RouteStatus.Completed)
        {
            throw new InvalidOperationException($"Маршрут {Name} уже завершён и не может быть отменён");
        }

        Status = RouteStatus.Cancelled;
        CompletionNotes = $"Отменён: {reason}";
        UpdatedAt = DateTime.UtcNow;

        // Если маршрут был в процессе, вернуть транспорт в активное состояние
        if (Vehicle?.IsOnRoute == true)
        {
            Vehicle.CompleteRoute();
        }
    }

    /// <summary>
    /// Приостановить маршрут
    /// </summary>
    public void Pause()
    {
        if (Status != RouteStatus.InProgress)
        {
            throw new InvalidOperationException($"Маршрут {Name} не может быть приостановлен");
        }

        Status = RouteStatus.Paused;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Возобновить маршрут
    /// </summary>
    public void Resume()
    {
        if (Status != RouteStatus.Paused)
        {
            throw new InvalidOperationException($"Маршрут {Name} не приостановлен");
        }

        Status = RouteStatus.InProgress;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Добавить точку на маршрут
    /// </summary>
    public void AddPoint(RoutePoint point)
    {
        if (Status != RouteStatus.Planned)
        {
            throw new InvalidOperationException("Невозможно добавить точку после начала маршрута");
        }

        RoutePoints.Add(point);
        UpdatedAt = DateTime.UtcNow;
    }
}