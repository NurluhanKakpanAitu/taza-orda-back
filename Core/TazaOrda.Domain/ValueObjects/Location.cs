namespace TazaOrda.Domain.ValueObjects;

/// <summary>
/// Value Object для хранения географических координат
/// </summary>
public class Location
{
    /// <summary>
    /// Широта (Latitude)
    /// </summary>
    public double Latitude { get; private set; }

    /// <summary>
    /// Долгота (Longitude)
    /// </summary>
    public double Longitude { get; private set; }

    /// <summary>
    /// Конструктор для создания геолокации
    /// </summary>
    /// <param name="latitude">Широта</param>
    /// <param name="longitude">Долгота</param>
    public Location(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
        {
            throw new ArgumentException("Широта должна быть в диапазоне от -90 до 90", nameof(latitude));
        }

        if (longitude < -180 || longitude > 180)
        {
            throw new ArgumentException("Долгота должна быть в диапазоне от -180 до 180", nameof(longitude));
        }

        Latitude = latitude;
        Longitude = longitude;
    }

    /// <summary>
    /// Форматированное представление координат
    /// </summary>
    public string Coordinates => $"{Latitude:F6}, {Longitude:F6}";

    /// <summary>
    /// Проверка на равенство двух локаций
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not Location other)
            return false;

        return Math.Abs(Latitude - other.Latitude) < 0.000001 &&
               Math.Abs(Longitude - other.Longitude) < 0.000001;
    }

    /// <summary>
    /// Получение хеш-кода
    /// </summary>
    public override int GetHashCode()
    {
        return HashCode.Combine(Latitude, Longitude);
    }

    /// <summary>
    /// Строковое представление локации
    /// </summary>
    public override string ToString() => Coordinates;
}