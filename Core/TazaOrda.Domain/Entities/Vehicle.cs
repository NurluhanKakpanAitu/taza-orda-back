using TazaOrda.Domain.Common;
using TazaOrda.Domain.Enums;
using TazaOrda.Domain.ValueObjects;

namespace TazaOrda.Domain.Entities;

/// <summary>
/// Транспортное средство (единица техники для вывоза мусора)
/// </summary>
public class Vehicle : BaseEntity
{
    /// <summary>
    /// Государственный номер транспортного средства
    /// </summary>
    public string LicensePlate { get; set; } = string.Empty;

    /// <summary>
    /// Тип транспортного средства
    /// </summary>
    public VehicleType Type { get; set; }

    /// <summary>
    /// ID GPS-трекера
    /// </summary>
    public string? GpsTrackerId { get; set; }

    /// <summary>
    /// Статус транспортного средства
    /// </summary>
    public VehicleStatus Status { get; set; } = VehicleStatus.Active;

    /// <summary>
    /// Марка/модель транспортного средства
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Год выпуска
    /// </summary>
    public int? ManufactureYear { get; set; }

    /// <summary>
    /// Грузоподъёмность в тоннах
    /// </summary>
    public double? CapacityInTons { get; set; }

    /// <summary>
    /// Текущая геолокация (если GPS-трекер активен)
    /// </summary>
    public Location? CurrentLocation { get; set; }

    /// <summary>
    /// Последнее обновление местоположения
    /// </summary>
    public DateTime? LastLocationUpdate { get; set; }

    /// <summary>
    /// Пробег в километрах
    /// </summary>
    public double? MileageInKm { get; set; }

    /// <summary>
    /// Дата последнего технического обслуживания
    /// </summary>
    public DateTime? LastMaintenanceDate { get; set; }

    /// <summary>
    /// Дата следующего планового технического обслуживания
    /// </summary>
    public DateTime? NextMaintenanceDate { get; set; }

    /// <summary>
    /// ID бригады, к которой приписан транспорт
    /// </summary>
    public Guid? AssignedTeamId { get; set; }

    /// <summary>
    /// Бригада, к которой приписан транспорт (навигационное свойство)
    /// </summary>
    public Team? AssignedTeam { get; set; }

    /// <summary>
    /// Коллекция маршрутов, выполненных на этом транспорте
    /// </summary>
    public ICollection<Route> Routes { get; set; } = new List<Route>();

    /// <summary>
    /// ID компании, к которой относится транспорт (опционально)
    /// </summary>
    public Guid? CompanyId { get; set; }

    /// <summary>
    /// Компания, к которой относится транспорт (навигационное свойство)
    /// </summary>
    public Company? Company { get; set; }

    /// <summary>
    /// Примечания/комментарии о транспорте
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Проверка, доступен ли транспорт для работы
    /// </summary>
    public bool IsAvailable => Status is VehicleStatus.Active or VehicleStatus.Idle;

    /// <summary>
    /// Проверка, находится ли транспорт на маршруте
    /// </summary>
    public bool IsOnRoute => Status == VehicleStatus.OnRoute;

    /// <summary>
    /// Проверка, требуется ли техническое обслуживание
    /// </summary>
    public bool RequiresMaintenance
    {
        get
        {
            if (NextMaintenanceDate == null)
                return false;

            return DateTime.UtcNow >= NextMaintenanceDate.Value.AddDays(-7); // За неделю до ТО
        }
    }

    /// <summary>
    /// Обновить местоположение транспорта
    /// </summary>
    public void UpdateLocation(Location location)
    {
        CurrentLocation = location;
        LastLocationUpdate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Отправить транспорт на маршрут
    /// </summary>
    public void StartRoute()
    {
        if (!IsAvailable)
        {
            throw new InvalidOperationException($"Транспорт {LicensePlate} недоступен для маршрута");
        }

        Status = VehicleStatus.OnRoute;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Завершить маршрут
    /// </summary>
    public void CompleteRoute()
    {
        if (!IsOnRoute)
        {
            throw new InvalidOperationException($"Транспорт {LicensePlate} не находится на маршруте");
        }

        Status = VehicleStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Отправить транспорт на ремонт
    /// </summary>
    public void SendToRepair(string? reason = null)
    {
        Status = VehicleStatus.InRepair;
        if (!string.IsNullOrWhiteSpace(reason))
        {
            Notes = $"{Notes}\n[{DateTime.UtcNow:yyyy-MM-dd}] Отправлен на ремонт: {reason}".Trim();
        }
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Вернуть транспорт из ремонта
    /// </summary>
    public void ReturnFromRepair()
    {
        if (Status != VehicleStatus.InRepair)
        {
            throw new InvalidOperationException($"Транспорт {LicensePlate} не находится в ремонте");
        }

        Status = VehicleStatus.Active;
        LastMaintenanceDate = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Назначить транспорт бригаде
    /// </summary>
    public void AssignToTeam(Team team)
    {
        AssignedTeamId = team.Id;
        AssignedTeam = team;
        UpdatedAt = DateTime.UtcNow;
    }
}