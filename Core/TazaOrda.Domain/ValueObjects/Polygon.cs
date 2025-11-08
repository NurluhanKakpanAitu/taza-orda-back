namespace TazaOrda.Domain.ValueObjects;

/// <summary>
/// Value Object для хранения геометрии района (многоугольник)
/// </summary>
public class Polygon
{
    /// <summary>
    /// Список координат, образующих границы многоугольника
    /// </summary>
    public IReadOnlyList<Location> Coordinates { get; private set; }

    /// <summary>
    /// Конструктор для создания многоугольника
    /// </summary>
    /// <param name="coordinates">Список координат границ</param>
    public Polygon(IEnumerable<Location> coordinates)
    {
        var coordinatesList = coordinates.ToList();

        if (coordinatesList.Count < 3)
        {
            throw new ArgumentException("Многоугольник должен содержать минимум 3 точки", nameof(coordinates));
        }

        Coordinates = coordinatesList.AsReadOnly();
    }

    /// <summary>
    /// Создание многоугольника из массива координат (широта, долгота)
    /// </summary>
    public static Polygon FromCoordinates(params (double latitude, double longitude)[] coordinates)
    {
        var locations = coordinates.Select(c => new Location(c.latitude, c.longitude));
        return new Polygon(locations);
    }

    /// <summary>
    /// Проверка, находится ли точка внутри многоугольника (алгоритм Ray Casting)
    /// </summary>
    public bool Contains(Location point)
    {
        var points = Coordinates.ToList();
        var inside = false;
        var j = points.Count - 1;

        for (var i = 0; i < points.Count; i++)
        {
            if ((points[i].Longitude > point.Longitude) != (points[j].Longitude > point.Longitude) &&
                point.Latitude < (points[j].Latitude - points[i].Latitude) *
                (point.Longitude - points[i].Longitude) /
                (points[j].Longitude - points[i].Longitude) + points[i].Latitude)
            {
                inside = !inside;
            }
            j = i;
        }

        return inside;
    }

    /// <summary>
    /// Получить центральную точку многоугольника
    /// </summary>
    public Location GetCenter()
    {
        var avgLatitude = Coordinates.Average(c => c.Latitude);
        var avgLongitude = Coordinates.Average(c => c.Longitude);
        return new Location(avgLatitude, avgLongitude);
    }

    /// <summary>
    /// Количество точек в многоугольнике
    /// </summary>
    public int PointsCount => Coordinates.Count;

    /// <summary>
    /// Строковое представление многоугольника
    /// </summary>
    public override string ToString()
    {
        return $"Polygon ({PointsCount} points)";
    }

    /// <summary>
    /// Проверка на равенство двух многоугольников
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not Polygon other)
            return false;

        if (Coordinates.Count != other.Coordinates.Count)
            return false;

        return Coordinates.SequenceEqual(other.Coordinates);
    }

    /// <summary>
    /// Получение хеш-кода
    /// </summary>
    public override int GetHashCode()
    {
        return Coordinates.Aggregate(0, (current, location) =>
            HashCode.Combine(current, location.GetHashCode()));
    }

    /// <summary>
    /// Проверка пересечения с другим полигоном
    /// </summary>
    public bool Intersects(Polygon other)
    {
        // Простая проверка: если хотя бы одна точка одного полигона находится внутри другого
        if (Coordinates.Any(point => other.Contains(point)))
            return true;

        if (other.Coordinates.Any(point => Contains(point)))
            return true;

        // Проверка пересечения рёбер (упрощённая версия)
        for (int i = 0; i < Coordinates.Count; i++)
        {
            var p1 = Coordinates[i];
            var p2 = Coordinates[(i + 1) % Coordinates.Count];

            for (int j = 0; j < other.Coordinates.Count; j++)
            {
                var q1 = other.Coordinates[j];
                var q2 = other.Coordinates[(j + 1) % other.Coordinates.Count];

                if (SegmentsIntersect(p1, p2, q1, q2))
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Проверка пересечения двух отрезков
    /// </summary>
    private static bool SegmentsIntersect(Location p1, Location p2, Location q1, Location q2)
    {
        double d1 = Direction(q1, q2, p1);
        double d2 = Direction(q1, q2, p2);
        double d3 = Direction(p1, p2, q1);
        double d4 = Direction(p1, p2, q2);

        if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &&
            ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0)))
            return true;

        return false;
    }

    /// <summary>
    /// Вычисление направления для проверки пересечения
    /// </summary>
    private static double Direction(Location p1, Location p2, Location p3)
    {
        return (p3.Latitude - p1.Latitude) * (p2.Longitude - p1.Longitude) -
               (p2.Latitude - p1.Latitude) * (p3.Longitude - p1.Longitude);
    }
}