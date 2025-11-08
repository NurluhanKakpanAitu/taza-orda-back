namespace TazaOrda.Domain.DTOs.Districts;

/// <summary>
/// DTO района для карты с GeoJSON
/// </summary>
public record DistrictMapDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public GeoJsonPolygon PolygonGeoJson { get; init; } = null!;
    public string? Color { get; init; }
    public int ReportsCount { get; init; }
    public string? Description { get; init; }
    public CenterPointDto? CenterPoint { get; init; }
}

/// <summary>
/// GeoJSON Polygon структура
/// </summary>
public record GeoJsonPolygon
{
    public string Type { get; init; } = "Polygon";
    public double[][][] Coordinates { get; init; } = Array.Empty<double[][]>();
}

/// <summary>
/// Центральная точка района
/// </summary>
public record CenterPointDto
{
    public double Lat { get; init; }
    public double Lng { get; init; }
}

/// <summary>
/// Статистика по району
/// </summary>
public record DistrictStatsDto
{
    public Guid DistrictId { get; init; }
    public string DistrictName { get; init; } = string.Empty;
    public int ReportsTotal { get; init; }
    public int ReportsDone { get; init; }
    public int ReportsActive { get; init; }
    public int ReportsNew { get; init; }
    public int ReportsInProgress { get; init; }
    public int ContainersCount { get; init; }
}

/// <summary>
/// Детальная информация о районе
/// </summary>
public record DistrictDetailDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public GeoJsonPolygon PolygonGeoJson { get; init; } = null!;
    public string? Color { get; init; }
    public CenterPointDto? CenterPoint { get; init; }
    public double? AreaInSquareKm { get; init; }
    public int? PopulationCount { get; init; }
    public bool IsActive { get; init; }
    public int ReportsCount { get; init; }
    public int ContainersCount { get; init; }
    public ResponsibleOperatorDto? ResponsibleOperator { get; init; }
}

/// <summary>
/// Ответственный оператор
/// </summary>
public record ResponsibleOperatorDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
}

/// <summary>
/// DTO для загрузки GeoJSON файла
/// </summary>
public record UploadGeoJsonRequest
{
    public string Name { get; init; } = string.Empty;
    public GeoJsonPolygon Geometry { get; init; } = null!;
    public string? Description { get; init; }
}

/// <summary>
/// GeoJSON Feature Collection для всех районов
/// </summary>
public record GeoJsonFeatureCollection
{
    public string Type { get; init; } = "FeatureCollection";
    public List<GeoJsonFeature> Features { get; init; } = new();
}

/// <summary>
/// GeoJSON Feature для одного района
/// </summary>
public record GeoJsonFeature
{
    public string Type { get; init; } = "Feature";
    public Guid Id { get; init; }
    public GeoJsonPolygon Geometry { get; init; } = null!;
    public FeatureProperties Properties { get; init; } = null!;
}

/// <summary>
/// Свойства GeoJSON Feature
/// </summary>
public record FeatureProperties
{
    public string Name { get; init; } = string.Empty;
    public int ReportsCount { get; init; }
    public string? Description { get; init; }
    public int? PopulationCount { get; init; }
}

/// <summary>
/// DTO для создания нового района
/// </summary>
public record CreateDistrictRequest
{
    public string Name { get; init; } = string.Empty;
    public GeoJsonPolygon PolygonGeoJson { get; init; } = null!;
    public string? Color { get; init; }
    public string? Description { get; init; }
    public double? AreaInSquareKm { get; init; }
    public int? PopulationCount { get; init; }
}

/// <summary>
/// DTO для обновления района
/// </summary>
public record UpdateDistrictRequest
{
    public string? Name { get; init; }
    public GeoJsonPolygon? PolygonGeoJson { get; init; }
    public string? Color { get; init; }
    public string? Description { get; init; }
    public double? AreaInSquareKm { get; init; }
    public int? PopulationCount { get; init; }
    public bool? IsActive { get; init; }
}

/// <summary>
/// DTO для импорта районов из GeoJSON с валидацией
/// </summary>
public record ImportDistrictsRequest
{
    public List<ImportDistrictItem> Districts { get; init; } = new();
    public bool ValidateOverlaps { get; init; } = true;
}

/// <summary>
/// Элемент импорта района
/// </summary>
public record ImportDistrictItem
{
    public string Name { get; init; } = string.Empty;
    public GeoJsonPolygon Geometry { get; init; } = null!;
    public string? Color { get; init; }
    public string? Description { get; init; }
}

/// <summary>
/// Результат валидации пересечений полигонов
/// </summary>
public record OverlapValidationResult
{
    public bool HasOverlaps { get; init; }
    public List<OverlapInfo> Overlaps { get; init; } = new();
}

/// <summary>
/// Информация о пересечении
/// </summary>
public record OverlapInfo
{
    public string District1Name { get; init; } = string.Empty;
    public string District2Name { get; init; } = string.Empty;
    public Guid? District1Id { get; init; }
    public Guid? District2Id { get; init; }
}

/// <summary>
/// Статистика по району с фильтрацией по датам
/// </summary>
public record DistrictStatsWithTimeDto
{
    public Guid DistrictId { get; init; }
    public string DistrictName { get; init; } = string.Empty;
    public int Total { get; init; }
    public int Active { get; init; }
    public int Done { get; init; }
    public double AvgResponseHours { get; init; }
}
