using TazaOrda.Domain.DTOs.Districts;
using TazaOrda.Domain.Entities;

namespace TazaOrda.Domain.Interfaces;

/// <summary>
/// Интерфейс сервиса для работы с районами
/// </summary>
public interface IDistrictService
{
    /// <summary>
    /// Получить все районы для карты
    /// </summary>
    Task<List<DistrictMapDto>> GetDistrictsForMapAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить статистику по районам
    /// </summary>
    Task<List<DistrictStatsDto>> GetDistrictsStatsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить статистику по районам с фильтрацией по датам
    /// </summary>
    Task<List<DistrictStatsWithTimeDto>> GetDistrictsStatsWithTimeAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить детальную информацию о районе
    /// </summary>
    Task<DistrictDetailDto?> GetDistrictDetailAsync(Guid districtId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Создать новый район
    /// </summary>
    Task<DistrictDetailDto> CreateDistrictAsync(CreateDistrictRequest request, User? currentUser, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновить район
    /// </summary>
    Task<DistrictDetailDto?> UpdateDistrictAsync(Guid districtId, UpdateDistrictRequest request, User? currentUser, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить район (мягкое удаление)
    /// </summary>
    Task<bool> DeleteDistrictAsync(Guid districtId, User? currentUser, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить GeoJSON FeatureCollection всех районов
    /// </summary>
    Task<GeoJsonFeatureCollection> GetDistrictsGeoJsonAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Экспорт районов в GeoJSON
    /// </summary>
    Task<GeoJsonFeatureCollection> ExportDistrictsGeoJsonAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Импорт районов из GeoJSON с валидацией
    /// </summary>
    Task<OverlapValidationResult> ImportDistrictsFromGeoJsonAsync(ImportDistrictsRequest request, User? currentUser, CancellationToken cancellationToken = default);

    /// <summary>
    /// Загрузить районы из GeoJSON (старый метод для обратной совместимости)
    /// </summary>
    Task UploadDistrictsFromGeoJsonAsync(List<UploadGeoJsonRequest> districts, CancellationToken cancellationToken = default);

    /// <summary>
    /// Валидация пересечений полигонов
    /// </summary>
    Task<OverlapValidationResult> ValidatePolygonOverlapsAsync(List<ImportDistrictItem> districts, CancellationToken cancellationToken = default);

    /// <summary>
    /// Определить район по координатам
    /// </summary>
    Task<DistrictMapDto?> FindDistrictByCoordinatesAsync(double latitude, double longitude, CancellationToken cancellationToken = default);
}
