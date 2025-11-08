using TazaOrda.Domain.Common;
using TazaOrda.Domain.ValueObjects;

namespace TazaOrda.Domain.Entities;

/// <summary>
/// Район/локация города для аналитики и фильтров
/// </summary>
public class District : BaseEntity
{
    /// <summary>
    /// Название района
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Геометрия района (многоугольник границ)
    /// </summary>
    public Polygon? Geometry { get; set; }

    /// <summary>
    /// ID ответственного оператора/компании
    /// </summary>
    public Guid? ResponsibleOperatorId { get; set; }

    /// <summary>
    /// Ответственный оператор/компания (навигационное свойство)
    /// </summary>
    public User? ResponsibleOperator { get; set; }

    /// <summary>
    /// ID компании, ответственной за район (опционально)
    /// </summary>
    public Guid? CompanyId { get; set; }

    /// <summary>
    /// Компания, ответственная за район (навигационное свойство)
    /// </summary>
    public Company? Company { get; set; }

    /// <summary>
    /// Описание района (опционально)
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Площадь района в квадратных километрах (опционально)
    /// </summary>
    public double? AreaInSquareKm { get; set; }

    /// <summary>
    /// Примерная численность населения района
    /// </summary>
    public int? PopulationCount { get; set; }

    /// <summary>
    /// Признак активности района
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Цвет района для отображения на карте (hex, например: #FF5733)
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Центральная точка района (вычисляется на основе геометрии)
    /// </summary>
    public Location? CenterPoint => Geometry?.GetCenter();

    /// <summary>
    /// Коллекция обращений в этом районе
    /// </summary>
    public ICollection<Report> Reports { get; set; } = new List<Report>();

    /// <summary>
    /// Коллекция контейнеров в этом районе
    /// </summary>
    public ICollection<Container> Containers { get; set; } = new List<Container>();

    /// <summary>
    /// Проверка, находится ли заданная локация в пределах района
    /// </summary>
    public bool ContainsLocation(Location location)
    {
        return Geometry?.Contains(location) ?? false;
    }

    /// <summary>
    /// Назначить ответственного оператора
    /// </summary>
    public void AssignOperator(User operatorUser)
    {
        if (!operatorUser.IsEmployee)
            throw new InvalidOperationException("Только сотрудники компании могут быть назначены ответственными за район");

        ResponsibleOperatorId = operatorUser.Id;
        ResponsibleOperator = operatorUser;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Деактивировать район
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Активировать район
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}