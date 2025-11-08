using TazaOrda.Domain.Common;

namespace TazaOrda.Domain.Entities;

/// <summary>
/// Компания/подрядчик, выполняющая работы по уборке
/// </summary>
public class Company : BaseEntity
{
    /// <summary>
    /// Название компании
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Полное юридическое название
    /// </summary>
    public string? LegalName { get; set; }

    /// <summary>
    /// БИН (Бизнес-идентификационный номер)
    /// </summary>
    public string? BusinessIdentificationNumber { get; set; }

    /// <summary>
    /// Юридический адрес
    /// </summary>
    public string? LegalAddress { get; set; }

    /// <summary>
    /// Контактный телефон
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Email
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Веб-сайт
    /// </summary>
    public string? Website { get; set; }

    /// <summary>
    /// Описание компании
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Логотип компании
    /// </summary>
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Районы, за которые отвечает компания
    /// </summary>
    public ICollection<District> ResponsibleDistricts { get; set; } = new List<District>();

    /// <summary>
    /// Сотрудники компании
    /// </summary>
    public ICollection<User> Employees { get; set; } = new List<User>();

    /// <summary>
    /// Бригады компании
    /// </summary>
    public ICollection<Team> Teams { get; set; } = new List<Team>();

    /// <summary>
    /// Транспорт компании
    /// </summary>
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

    /// <summary>
    /// ID директора/руководителя компании
    /// </summary>
    public Guid? DirectorId { get; set; }

    /// <summary>
    /// Директор/руководитель (навигационное свойство)
    /// </summary>
    public User? Director { get; set; }

    /// <summary>
    /// Признак активности компании
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Дата начала контракта
    /// </summary>
    public DateTime? ContractStartDate { get; set; }

    /// <summary>
    /// Дата окончания контракта
    /// </summary>
    public DateTime? ContractEndDate { get; set; }

    /// <summary>
    /// Рейтинг компании (средняя оценка по отзывам)
    /// </summary>
    public double? Rating { get; set; }

    /// <summary>
    /// Количество выполненных заявок
    /// </summary>
    public int CompletedReportsCount { get; set; } = 0;

    /// <summary>
    /// Количество активных маршрутов
    /// </summary>
    public int ActiveRoutesCount { get; set; } = 0;

    /// <summary>
    /// Примечания о компании
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Количество сотрудников
    /// </summary>
    public int EmployeesCount => Employees.Count;

    /// <summary>
    /// Количество бригад
    /// </summary>
    public int TeamsCount => Teams.Count;

    /// <summary>
    /// Количество единиц транспорта
    /// </summary>
    public int VehiclesCount => Vehicles.Count;

    /// <summary>
    /// Количество районов обслуживания
    /// </summary>
    public int DistrictsCount => ResponsibleDistricts.Count;

    /// <summary>
    /// Проверка, действует ли контракт
    /// </summary>
    public bool IsContractValid
    {
        get
        {
            if (!ContractStartDate.HasValue || !ContractEndDate.HasValue)
                return IsActive;

            var now = DateTime.UtcNow;
            return IsActive && now >= ContractStartDate.Value && now <= ContractEndDate.Value;
        }
    }

    /// <summary>
    /// Проверка, истекает ли контракт скоро (в течение 30 дней)
    /// </summary>
    public bool IsContractExpiringSoon
    {
        get
        {
            if (!ContractEndDate.HasValue)
                return false;

            var daysUntilExpiry = (ContractEndDate.Value - DateTime.UtcNow).Days;
            return daysUntilExpiry > 0 && daysUntilExpiry <= 30;
        }
    }

    /// <summary>
    /// Добавить сотрудника в компанию
    /// </summary>
    public void AddEmployee(User user)
    {
        if (!user.IsEmployee)
        {
            throw new InvalidOperationException("Пользователь должен иметь роль сотрудника");
        }

        if (Employees.Any(e => e.Id == user.Id))
        {
            throw new InvalidOperationException("Сотрудник уже добавлен в компанию");
        }

        Employees.Add(user);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Удалить сотрудника из компании
    /// </summary>
    public void RemoveEmployee(Guid userId)
    {
        var employee = Employees.FirstOrDefault(e => e.Id == userId);
        if (employee == null)
        {
            throw new InvalidOperationException("Сотрудник не найден");
        }

        if (DirectorId == userId)
        {
            throw new InvalidOperationException("Нельзя удалить директора компании");
        }

        Employees.Remove(employee);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Назначить директора компании
    /// </summary>
    public void AssignDirector(User user)
    {
        if (!user.IsEmployee)
        {
            throw new InvalidOperationException("Директором может быть только сотрудник компании");
        }

        // Если пользователь не в списке сотрудников, добавить его
        if (!Employees.Any(e => e.Id == user.Id))
        {
            AddEmployee(user);
        }

        DirectorId = user.Id;
        Director = user;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Добавить район обслуживания
    /// </summary>
    public void AddDistrict(District district)
    {
        if (ResponsibleDistricts.Any(d => d.Id == district.Id))
        {
            throw new InvalidOperationException("Район уже добавлен");
        }

        ResponsibleDistricts.Add(district);
        district.AssignOperator(Director ?? throw new InvalidOperationException("У компании должен быть директор"));
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Удалить район обслуживания
    /// </summary>
    public void RemoveDistrict(Guid districtId)
    {
        var district = ResponsibleDistricts.FirstOrDefault(d => d.Id == districtId);
        if (district == null)
        {
            throw new InvalidOperationException("Район не найден");
        }

        ResponsibleDistricts.Remove(district);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Обновить рейтинг компании
    /// </summary>
    public void UpdateRating(double newRating)
    {
        if (newRating < 1 || newRating > 5)
        {
            throw new ArgumentException("Рейтинг должен быть от 1 до 5", nameof(newRating));
        }

        Rating = newRating;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Увеличить счётчик выполненных заявок
    /// </summary>
    public void IncrementCompletedReports()
    {
        CompletedReportsCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Деактивировать компанию
    /// </summary>
    public void Deactivate(string? reason = null)
    {
        IsActive = false;
        if (!string.IsNullOrWhiteSpace(reason))
        {
            Notes = $"{Notes}\n[{DateTime.UtcNow:yyyy-MM-dd}] Деактивирована: {reason}".Trim();
        }
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Активировать компанию
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}