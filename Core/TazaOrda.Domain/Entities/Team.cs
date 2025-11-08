using TazaOrda.Domain.Common;

namespace TazaOrda.Domain.Entities;

/// <summary>
/// Бригада/экипаж сотрудников для выполнения работ
/// </summary>
public class Team : BaseEntity
{
    /// <summary>
    /// Название бригады
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Описание бригады
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// ID компании, к которой относится бригада (опционально)
    /// </summary>
    public Guid? CompanyId { get; set; }

    /// <summary>
    /// Компания, к которой относится бригада (навигационное свойство)
    /// </summary>
    public Company? Company { get; set; }

    /// <summary>
    /// ID руководителя бригады (бригадир)
    /// </summary>
    public Guid? TeamLeaderId { get; set; }

    /// <summary>
    /// Руководитель бригады (навигационное свойство)
    /// </summary>
    public User? TeamLeader { get; set; }

    /// <summary>
    /// Состав бригады (список сотрудников)
    /// </summary>
    public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();

    /// <summary>
    /// ID транспортного средства, приписанного к бригаде
    /// </summary>
    public Guid? VehicleId { get; set; }

    /// <summary>
    /// Транспортное средство бригады (навигационное свойство)
    /// </summary>
    public Vehicle? Vehicle { get; set; }

    /// <summary>
    /// ID текущего активного маршрута
    /// </summary>
    public Guid? ActiveRouteId { get; set; }

    /// <summary>
    /// Текущий активный маршрут (навигационное свойство)
    /// </summary>
    public Route? ActiveRoute { get; set; }

    /// <summary>
    /// Коллекция всех маршрутов бригады
    /// </summary>
    public ICollection<Route> Routes { get; set; } = new List<Route>();

    /// <summary>
    /// Признак активности бригады
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Контактный телефон бригады
    /// </summary>
    public string? ContactPhone { get; set; }

    /// <summary>
    /// Количество членов бригады
    /// </summary>
    public int MembersCount => Members.Count;

    /// <summary>
    /// Проверка, занята ли бригада в данный момент
    /// </summary>
    public bool IsBusy => ActiveRoute != null && ActiveRoute.IsActive;

    /// <summary>
    /// Проверка, доступна ли бригада для назначения на маршрут
    /// </summary>
    public bool IsAvailable => IsActive && !IsBusy && MembersCount > 0;

    /// <summary>
    /// Добавить сотрудника в бригаду
    /// </summary>
    public void AddMember(User user, string? role = null)
    {
        if (!user.IsEmployee)
        {
            throw new InvalidOperationException("В бригаду могут быть добавлены только сотрудники компании");
        }

        var member = new TeamMember
        {
            TeamId = Id,
            Team = this,
            UserId = user.Id,
            User = user,
            Role = role,
            JoinedAt = DateTime.UtcNow
        };

        Members.Add(member);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Удалить сотрудника из бригады
    /// </summary>
    public void RemoveMember(Guid userId)
    {
        var member = Members.FirstOrDefault(m => m.UserId == userId);
        if (member == null)
        {
            throw new InvalidOperationException("Сотрудник не найден в составе бригады");
        }

        Members.Remove(member);
        UpdatedAt = DateTime.UtcNow;

        // Если удалили руководителя, сбросить ссылку
        if (TeamLeaderId == userId)
        {
            TeamLeaderId = null;
            TeamLeader = null;
        }
    }

    /// <summary>
    /// Назначить руководителя бригады
    /// </summary>
    public void AssignTeamLeader(User user)
    {
        if (!user.IsEmployee)
        {
            throw new InvalidOperationException("Руководителем бригады может быть только сотрудник компании");
        }

        // Если пользователь не в составе бригады, добавить его
        if (!Members.Any(m => m.UserId == user.Id))
        {
            AddMember(user, "Бригадир");
        }

        TeamLeaderId = user.Id;
        TeamLeader = user;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Назначить транспорт бригаде
    /// </summary>
    public void AssignVehicle(Vehicle vehicle)
    {
        VehicleId = vehicle.Id;
        Vehicle = vehicle;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Начать маршрут
    /// </summary>
    public void StartRoute(Route route)
    {
        if (!IsAvailable)
        {
            throw new InvalidOperationException($"Бригада {Name} недоступна для начала маршрута");
        }

        if (route.TeamId != Id)
        {
            throw new InvalidOperationException("Маршрут не назначен этой бригаде");
        }

        ActiveRouteId = route.Id;
        ActiveRoute = route;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Завершить текущий маршрут
    /// </summary>
    public void CompleteActiveRoute()
    {
        if (ActiveRoute == null)
        {
            throw new InvalidOperationException($"У бригады {Name} нет активного маршрута");
        }

        ActiveRouteId = null;
        ActiveRoute = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Деактивировать бригаду
    /// </summary>
    public void Deactivate()
    {
        if (IsBusy)
        {
            throw new InvalidOperationException("Невозможно деактивировать бригаду с активным маршрутом");
        }

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Активировать бригаду
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}