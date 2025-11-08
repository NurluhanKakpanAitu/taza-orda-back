using Microsoft.AspNetCore.Mvc;
using TazaOrda.API.Attributes;
using TazaOrda.Domain.DTOs.Districts;
using TazaOrda.Domain.Entities;
using TazaOrda.Domain.Interfaces;

namespace TazaOrda.API.Controllers;

/// <summary>
/// Контроллер для работы с районами города
/// </summary>
[ApiController]
[Route("api/districts")]
public class DistrictsController : ControllerBase
{
    private readonly IDistrictService _districtService;
    private readonly ILogger<DistrictsController> _logger;

    public DistrictsController(IDistrictService districtService, ILogger<DistrictsController> logger)
    {
        _districtService = districtService;
        _logger = logger;
    }

    /// <summary>
    /// Получить все районы для отображения на карте
    /// GET /districts
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<DistrictMapDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDistricts(CancellationToken cancellationToken)
    {
        try
        {
            var districts = await _districtService.GetDistrictsForMapAsync(cancellationToken);
            return Ok(districts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting districts");
            return StatusCode(500, new { message = "Произошла ошибка при получении районов" });
        }
    }

    /// <summary>
    /// Получить статистику по всем районам
    /// GET /districts/stats
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(List<DistrictStatsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDistrictsStats(CancellationToken cancellationToken)
    {
        try
        {
            var stats = await _districtService.GetDistrictsStatsAsync(cancellationToken);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting districts stats");
            return StatusCode(500, new { message = "Произошла ошибка при получении статистики" });
        }
    }

    /// <summary>
    /// Получить детальную информацию о районе
    /// GET /districts/{id}
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DistrictDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDistrict(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var district = await _districtService.GetDistrictDetailAsync(id, cancellationToken);

            if (district == null)
            {
                return NotFound(new { message = "Район не найден" });
            }

            return Ok(district);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting district {DistrictId}", id);
            return StatusCode(500, new { message = "Произошла ошибка при получении района" });
        }
    }

    /// <summary>
    /// Получить GeoJSON FeatureCollection всех районов
    /// GET /districts/geojson
    /// </summary>
    [HttpGet("geojson")]
    [ProducesResponseType(typeof(GeoJsonFeatureCollection), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDistrictsGeoJson(CancellationToken cancellationToken)
    {
        try
        {
            var geoJson = await _districtService.GetDistrictsGeoJsonAsync(cancellationToken);
            return Ok(geoJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting districts GeoJSON");
            return StatusCode(500, new { message = "Произошла ошибка при получении GeoJSON" });
        }
    }

    /// <summary>
    /// Загрузить районы из GeoJSON
    /// POST /districts/upload
    /// </summary>
    [HttpPost("upload")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadDistricts([FromBody] List<UploadGeoJsonRequest> districts, CancellationToken cancellationToken)
    {
        try
        {
            if (districts == null || districts.Count == 0)
            {
                return BadRequest(new { message = "Список районов не может быть пустым" });
            }

            await _districtService.UploadDistrictsFromGeoJsonAsync(districts, cancellationToken);
            return Ok(new { message = $"Успешно загружено районов: {districts.Count}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading districts");
            return StatusCode(500, new { message = "Произошла ошибка при загрузке районов" });
        }
    }

    /// <summary>
    /// Определить район по координатам
    /// GET /districts/find?lat=44.85&lng=65.50
    /// </summary>
    [HttpGet("find")]
    [ProducesResponseType(typeof(DistrictMapDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FindDistrictByCoordinates([FromQuery] double lat, [FromQuery] double lng, CancellationToken cancellationToken)
    {
        try
        {
            var district = await _districtService.FindDistrictByCoordinatesAsync(lat, lng, cancellationToken);

            if (district == null)
            {
                return NotFound(new { message = "Район не найден для указанных координат" });
            }

            return Ok(district);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding district by coordinates {Lat}, {Lng}", lat, lng);
            return StatusCode(500, new { message = "Произошла ошибка при поиске района" });
        }
    }

    /// <summary>
    /// Создать новый район
    /// POST /districts
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(DistrictDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateDistrict([FromBody] CreateDistrictRequest request, CancellationToken cancellationToken)
    {
        var user = HttpContext.Items["User"] as User;
        if (user == null)
            return Unauthorized(new { message = "Пользователь не аутентифицирован" });

        try
        {
            var district = await _districtService.CreateDistrictAsync(request, user, cancellationToken);
            return CreatedAtAction(nameof(GetDistrict), new { id = district.Id }, district);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid district geometry");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating district");
            return StatusCode(500, new { message = "Произошла ошибка при создании района" });
        }
    }

    /// <summary>
    /// Обновить район
    /// PATCH /districts/{id}
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(DistrictDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateDistrict(Guid id, [FromBody] UpdateDistrictRequest request, CancellationToken cancellationToken)
    {
        var user = HttpContext.Items["User"] as User;
        if (user == null)
            return Unauthorized(new { message = "Пользователь не аутентифицирован" });

        try
        {
            var district = await _districtService.UpdateDistrictAsync(id, request, user, cancellationToken);

            if (district == null)
            {
                return NotFound(new { message = "Район не найден" });
            }

            return Ok(district);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating district {DistrictId}", id);
            return StatusCode(500, new { message = "Произошла ошибка при обновлении района" });
        }
    }

    /// <summary>
    /// Удалить район (мягкое удаление)
    /// DELETE /districts/{id}
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteDistrict(Guid id, CancellationToken cancellationToken)
    {
        var user = HttpContext.Items["User"] as User;
        if (user == null)
            return Unauthorized(new { message = "Пользователь не аутентифицирован" });

        try
        {
            var success = await _districtService.DeleteDistrictAsync(id, user, cancellationToken);

            if (!success)
            {
                return NotFound(new { message = "Район не найден" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting district {DistrictId}", id);
            return StatusCode(500, new { message = "Произошла ошибка при удалении района" });
        }
    }

    /// <summary>
    /// Получить статистику по районам с фильтрацией по датам
    /// GET /districts/stats/time?from=2024-01-01&to=2024-12-31
    /// </summary>
    [HttpGet("stats/time")]
    [ProducesResponseType(typeof(List<DistrictStatsWithTimeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDistrictsStatsWithTime(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        try
        {
            var stats = await _districtService.GetDistrictsStatsWithTimeAsync(from, to, cancellationToken);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting districts stats with time filter");
            return StatusCode(500, new { message = "Произошла ошибка при получении статистики" });
        }
    }

    /// <summary>
    /// Экспорт всех районов в GeoJSON
    /// GET /districts/export-geojson
    /// </summary>
    [HttpGet("export-geojson")]
    [ProducesResponseType(typeof(GeoJsonFeatureCollection), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportDistrictsGeoJson(CancellationToken cancellationToken)
    {
        try
        {
            var geoJson = await _districtService.ExportDistrictsGeoJsonAsync(cancellationToken);
            return Ok(geoJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting districts GeoJSON");
            return StatusCode(500, new { message = "Произошла ошибка при экспорте GeoJSON" });
        }
    }

    /// <summary>
    /// Импорт районов из GeoJSON с валидацией пересечений
    /// POST /districts/import-geojson
    /// </summary>
    [HttpPost("import-geojson")]
    [Authorize]
    [ProducesResponseType(typeof(OverlapValidationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ImportDistrictsGeoJson([FromBody] ImportDistrictsRequest request, CancellationToken cancellationToken)
    {
        var user = HttpContext.Items["User"] as User;
        if (user == null)
            return Unauthorized(new { message = "Пользователь не аутентифицирован" });

        try
        {
            if (request.Districts == null || request.Districts.Count == 0)
            {
                return BadRequest(new { message = "Список районов не может быть пустым" });
            }

            var result = await _districtService.ImportDistrictsFromGeoJsonAsync(request, user, cancellationToken);

            if (result.HasOverlaps)
            {
                return BadRequest(new
                {
                    message = "Обнаружены пересечения между районами",
                    overlaps = result.Overlaps
                });
            }

            return Ok(new { message = $"Успешно импортировано районов: {request.Districts.Count}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing districts from GeoJSON");
            return StatusCode(500, new { message = "Произошла ошибка при импорте районов" });
        }
    }

    /// <summary>
    /// Валидация пересечений полигонов районов
    /// POST /districts/validate-overlaps
    /// </summary>
    [HttpPost("validate-overlaps")]
    [ProducesResponseType(typeof(OverlapValidationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ValidatePolygonOverlaps([FromBody] List<ImportDistrictItem> districts, CancellationToken cancellationToken)
    {
        try
        {
            if (districts == null || districts.Count == 0)
            {
                return BadRequest(new { message = "Список районов не может быть пустым" });
            }

            var result = await _districtService.ValidatePolygonOverlapsAsync(districts, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating polygon overlaps");
            return StatusCode(500, new { message = "Произошла ошибка при валидации пересечений" });
        }
    }
}
