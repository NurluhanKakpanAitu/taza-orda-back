using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TazaOrda.Domain.DTOs.Districts;
using TazaOrda.Domain.Entities;
using TazaOrda.Domain.Enums;
using TazaOrda.Domain.Interfaces;
using TazaOrda.Domain.ValueObjects;
using TazaOrda.Infrastructure.Persistence;

namespace TazaOrda.Infrastructure.Services.Districts;

/// <summary>
/// Реализация сервиса для работы с районами
/// </summary>
public class DistrictService(TazaOrdaDbContext context) : IDistrictService
{
    public async Task<List<DistrictMapDto>> GetDistrictsForMapAsync(CancellationToken cancellationToken = default)
    {
        var districts = await context.Districts
            .Where(d => d.IsActive)
            .Include(d => d.Reports)
            .Select(d => new DistrictMapDto
            {
                Id = d.Id,
                Name = d.Name,
                PolygonGeoJson = ConvertPolygonToGeoJson(d.Geometry),
                Color = d.Color,
                ReportsCount = d.Reports.Count,
                Description = d.Description,
                CenterPoint = d.Geometry != null ? new CenterPointDto
                {
                    Lat = d.Geometry.GetCenter().Latitude,
                    Lng = d.Geometry.GetCenter().Longitude
                } : null
            })
            .ToListAsync(cancellationToken);

        return districts;
    }

    public async Task<List<DistrictStatsDto>> GetDistrictsStatsAsync(CancellationToken cancellationToken = default)
    {
        var stats = await context.Districts
            .Where(d => d.IsActive)
            .Select(d => new DistrictStatsDto
            {
                DistrictId = d.Id,
                DistrictName = d.Name,
                ReportsTotal = d.Reports.Count,
                ReportsDone = d.Reports.Count(r => r.Status == ReportStatus.Completed || r.Status == ReportStatus.Closed),
                ReportsActive = d.Reports.Count(r => r.Status == ReportStatus.New || r.Status == ReportStatus.InProgress),
                ReportsNew = d.Reports.Count(r => r.Status == ReportStatus.New),
                ReportsInProgress = d.Reports.Count(r => r.Status == ReportStatus.InProgress),
                ContainersCount = d.Containers.Count
            })
            .ToListAsync(cancellationToken);

        return stats;
    }

    public async Task<DistrictDetailDto?> GetDistrictDetailAsync(Guid districtId, CancellationToken cancellationToken = default)
    {
        var district = await context.Districts
            .Include(d => d.ResponsibleOperator)
            .Include(d => d.Reports)
            .Include(d => d.Containers)
            .FirstOrDefaultAsync(d => d.Id == districtId, cancellationToken);

        if (district == null)
        {
            return null;
        }

        return new DistrictDetailDto
        {
            Id = district.Id,
            Name = district.Name,
            Description = district.Description,
            PolygonGeoJson = ConvertPolygonToGeoJson(district.Geometry),
            Color = district.Color,
            CenterPoint = district.CenterPoint != null ? new CenterPointDto
            {
                Lat = district.CenterPoint.Latitude,
                Lng = district.CenterPoint.Longitude
            } : null,
            AreaInSquareKm = district.AreaInSquareKm,
            PopulationCount = district.PopulationCount,
            IsActive = district.IsActive,
            ReportsCount = district.Reports.Count,
            ContainersCount = district.Containers.Count,
            ResponsibleOperator = district.ResponsibleOperator != null ? new ResponsibleOperatorDto
            {
                Id = district.ResponsibleOperator.Id,
                Name = district.ResponsibleOperator.FullName,
                PhoneNumber = district.ResponsibleOperator.PhoneNumber
            } : null
        };
    }

    public async Task<GeoJsonFeatureCollection> GetDistrictsGeoJsonAsync(CancellationToken cancellationToken = default)
    {
        var districts = await context.Districts
            .Where(d => d.IsActive)
            .Include(d => d.Reports)
            .ToListAsync(cancellationToken);

        var features = districts.Select(d => new GeoJsonFeature
        {
            Id = d.Id,
            Geometry = ConvertPolygonToGeoJson(d.Geometry),
            Properties = new FeatureProperties
            {
                Name = d.Name,
                ReportsCount = d.Reports.Count,
                Description = d.Description,
                PopulationCount = d.PopulationCount
            }
        }).ToList();

        return new GeoJsonFeatureCollection
        {
            Features = features
        };
    }

    public async Task UploadDistrictsFromGeoJsonAsync(List<UploadGeoJsonRequest> districts, CancellationToken cancellationToken = default)
    {
        foreach (var districtDto in districts)
        {
            // Проверить, существует ли район с таким именем
            var existingDistrict = await context.Districts
                .FirstOrDefaultAsync(d => d.Name == districtDto.Name, cancellationToken);

            if (existingDistrict != null)
            {
                // Обновить геометрию
                existingDistrict.Geometry = ConvertGeoJsonToPolygon(districtDto.Geometry);
                existingDistrict.Description = districtDto.Description;
                existingDistrict.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Создать новый район
                var newDistrict = new District
                {
                    Name = districtDto.Name,
                    Geometry = ConvertGeoJsonToPolygon(districtDto.Geometry),
                    Description = districtDto.Description,
                    IsActive = true
                };

                context.Districts.Add(newDistrict);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<DistrictMapDto?> FindDistrictByCoordinatesAsync(double latitude, double longitude, CancellationToken cancellationToken = default)
    {
        var location = new Location(latitude, longitude);
        var districts = await context.Districts
            .Where(d => d.IsActive)
            .Include(d => d.Reports)
            .ToListAsync(cancellationToken);

        var district = districts.FirstOrDefault(d => d.ContainsLocation(location));

        if (district == null)
        {
            return null;
        }

        return new DistrictMapDto
        {
            Id = district.Id,
            Name = district.Name,
            PolygonGeoJson = ConvertPolygonToGeoJson(district.Geometry),
            Color = district.Color,
            ReportsCount = district.Reports.Count,
            Description = district.Description,
            CenterPoint = district.CenterPoint != null ? new CenterPointDto
            {
                Lat = district.CenterPoint.Latitude,
                Lng = district.CenterPoint.Longitude
            } : null
        };
    }

    /// <summary>
    /// Преобразовать Polygon ValueObject в GeoJSON структуру
    /// </summary>
    private static GeoJsonPolygon ConvertPolygonToGeoJson(Polygon? polygon)
    {
        if (polygon == null)
        {
            return new GeoJsonPolygon();
        }

        // GeoJSON Polygon format: [[[lng, lat], [lng, lat], ...]]
        var coordinates = new[]
        {
            polygon.Coordinates.Select(c => new[] { c.Longitude, c.Latitude }).ToArray()
        };

        return new GeoJsonPolygon
        {
            Type = "Polygon",
            Coordinates = coordinates
        };
    }

    /// <summary>
    /// Преобразовать GeoJSON структуру в Polygon ValueObject
    /// </summary>
    private static Polygon? ConvertGeoJsonToPolygon(GeoJsonPolygon? geoJson)
    {
        if (geoJson == null || geoJson.Coordinates.Length == 0)
        {
            return null;
        }

        // GeoJSON format: [[[lng, lat], [lng, lat], ...]]
        var ring = geoJson.Coordinates[0];
        var locations = ring.Select(coord => new Location(coord[1], coord[0])).ToList();

        return new Polygon(locations);
    }

    public async Task<DistrictDetailDto> CreateDistrictAsync(CreateDistrictRequest request, User? currentUser, CancellationToken cancellationToken = default)
    {
        var polygon = ConvertGeoJsonToPolygon(request.PolygonGeoJson);

        if (polygon == null)
            throw new ArgumentException("Некорректная геометрия района");

        var district = new District
        {
            Name = request.Name,
            Geometry = polygon,
            Color = request.Color,
            Description = request.Description,
            AreaInSquareKm = request.AreaInSquareKm,
            PopulationCount = request.PopulationCount,
            IsActive = true
        };

        context.Districts.Add(district);

        // Создание записи аудита
        if (currentUser != null)
        {
            var auditLog = AuditLog.LogCreated(
                "District",
                district.Id,
                district.Name,
                currentUser,
                JsonSerializer.Serialize(new { request.Name, request.Color, request.Description })
            );
            context.AuditLogs.Add(auditLog);
        }

        await context.SaveChangesAsync(cancellationToken);

        return (await GetDistrictDetailAsync(district.Id, cancellationToken))!;
    }

    public async Task<DistrictDetailDto?> UpdateDistrictAsync(Guid districtId, UpdateDistrictRequest request, User? currentUser, CancellationToken cancellationToken = default)
    {
        var district = await context.Districts.FindAsync(new object[] { districtId }, cancellationToken);

        if (district == null)
            return null;

        var oldValues = JsonSerializer.Serialize(new
        {
            district.Name,
            district.Color,
            district.Description,
            district.IsActive
        });

        if (request.Name != null)
            district.Name = request.Name;

        if (request.PolygonGeoJson != null)
            district.Geometry = ConvertGeoJsonToPolygon(request.PolygonGeoJson);

        if (request.Color != null)
            district.Color = request.Color;

        if (request.Description != null)
            district.Description = request.Description;

        if (request.AreaInSquareKm.HasValue)
            district.AreaInSquareKm = request.AreaInSquareKm;

        if (request.PopulationCount.HasValue)
            district.PopulationCount = request.PopulationCount;

        if (request.IsActive.HasValue)
            district.IsActive = request.IsActive.Value;

        district.UpdatedAt = DateTime.UtcNow;

        // Создание записи аудита
        if (currentUser != null)
        {
            var newValues = JsonSerializer.Serialize(new
            {
                district.Name,
                district.Color,
                district.Description,
                district.IsActive
            });

            var auditLog = AuditLog.LogUpdated(
                "District",
                district.Id,
                district.Name,
                currentUser,
                oldValues,
                newValues
            );
            context.AuditLogs.Add(auditLog);
        }

        await context.SaveChangesAsync(cancellationToken);

        return await GetDistrictDetailAsync(districtId, cancellationToken);
    }

    public async Task<bool> DeleteDistrictAsync(Guid districtId, User? currentUser, CancellationToken cancellationToken = default)
    {
        var district = await context.Districts.FindAsync(new object[] { districtId }, cancellationToken);

        if (district == null)
            return false;

        var oldValues = JsonSerializer.Serialize(new
        {
            district.Name,
            district.IsActive
        });

        // Мягкое удаление
        district.Deactivate();

        // Создание записи аудита
        if (currentUser != null)
        {
            var auditLog = AuditLog.LogDeleted(
                "District",
                district.Id,
                district.Name,
                currentUser,
                oldValues
            );
            context.AuditLogs.Add(auditLog);
        }

        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<List<DistrictStatsWithTimeDto>> GetDistrictsStatsWithTimeAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var query = context.Districts
            .Where(d => d.IsActive)
            .AsQueryable();

        var stats = await query
            .Select(d => new
            {
                d.Id,
                d.Name,
                Reports = d.Reports.Where(r =>
                    (!from.HasValue || r.CreatedAt >= from.Value) &&
                    (!to.HasValue || r.CreatedAt <= to.Value)
                ).ToList()
            })
            .ToListAsync(cancellationToken);

        return stats.Select(s => new DistrictStatsWithTimeDto
        {
            DistrictId = s.Id,
            DistrictName = s.Name,
            Total = s.Reports.Count,
            Active = s.Reports.Count(r => r.Status == ReportStatus.New || r.Status == ReportStatus.InProgress),
            Done = s.Reports.Count(r => r.Status == ReportStatus.Completed || r.Status == ReportStatus.Closed),
            AvgResponseHours = s.Reports.Any(r => r.ClosedAt.HasValue)
                ? s.Reports
                    .Where(r => r.ClosedAt.HasValue)
                    .Average(r => (r.ClosedAt!.Value - r.CreatedAt).TotalHours)
                : 0
        }).ToList();
    }

    public async Task<GeoJsonFeatureCollection> ExportDistrictsGeoJsonAsync(CancellationToken cancellationToken = default)
    {
        return await GetDistrictsGeoJsonAsync(cancellationToken);
    }

    public async Task<OverlapValidationResult> ImportDistrictsFromGeoJsonAsync(ImportDistrictsRequest request, User? currentUser, CancellationToken cancellationToken = default)
    {
        // Валидация пересечений, если требуется
        if (request.ValidateOverlaps)
        {
            var validationResult = await ValidatePolygonOverlapsAsync(request.Districts, cancellationToken);

            if (validationResult.HasOverlaps)
            {
                return validationResult;
            }
        }

        // Импорт районов
        foreach (var districtDto in request.Districts)
        {
            var existingDistrict = await context.Districts
                .FirstOrDefaultAsync(d => d.Name == districtDto.Name, cancellationToken);

            if (existingDistrict != null)
            {
                // Обновить существующий район
                existingDistrict.Geometry = ConvertGeoJsonToPolygon(districtDto.Geometry);
                existingDistrict.Color = districtDto.Color;
                existingDistrict.Description = districtDto.Description;
                existingDistrict.UpdatedAt = DateTime.UtcNow;

                if (currentUser != null)
                {
                    var auditLog = AuditLog.LogUpdated(
                        "District",
                        existingDistrict.Id,
                        existingDistrict.Name,
                        currentUser,
                        newValues: "Updated from GeoJSON import"
                    );
                    context.AuditLogs.Add(auditLog);
                }
            }
            else
            {
                // Создать новый район
                var newDistrict = new District
                {
                    Name = districtDto.Name,
                    Geometry = ConvertGeoJsonToPolygon(districtDto.Geometry),
                    Color = districtDto.Color,
                    Description = districtDto.Description,
                    IsActive = true
                };

                context.Districts.Add(newDistrict);

                if (currentUser != null)
                {
                    var auditLog = AuditLog.LogCreated(
                        "District",
                        newDistrict.Id,
                        newDistrict.Name,
                        currentUser,
                        "Created from GeoJSON import"
                    );
                    context.AuditLogs.Add(auditLog);
                }
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        return new OverlapValidationResult { HasOverlaps = false };
    }

    public async Task<OverlapValidationResult> ValidatePolygonOverlapsAsync(List<ImportDistrictItem> districts, CancellationToken cancellationToken = default)
    {
        var overlaps = new List<OverlapInfo>();

        // Загрузить существующие районы
        var existingDistricts = await context.Districts
            .Where(d => d.IsActive)
            .ToListAsync(cancellationToken);

        // Проверить пересечения между новыми районами
        for (int i = 0; i < districts.Count; i++)
        {
            var polygon1 = ConvertGeoJsonToPolygon(districts[i].Geometry);
            if (polygon1 == null) continue;

            for (int j = i + 1; j < districts.Count; j++)
            {
                var polygon2 = ConvertGeoJsonToPolygon(districts[j].Geometry);
                if (polygon2 == null) continue;

                if (polygon1.Intersects(polygon2))
                {
                    overlaps.Add(new OverlapInfo
                    {
                        District1Name = districts[i].Name,
                        District2Name = districts[j].Name
                    });
                }
            }

            // Проверить пересечения с существующими районами
            foreach (var existingDistrict in existingDistricts)
            {
                if (existingDistrict.Name == districts[i].Name)
                    continue; // Пропустить, если обновляем существующий район

                if (existingDistrict.Geometry != null && polygon1.Intersects(existingDistrict.Geometry))
                {
                    overlaps.Add(new OverlapInfo
                    {
                        District1Name = districts[i].Name,
                        District2Name = existingDistrict.Name,
                        District2Id = existingDistrict.Id
                    });
                }
            }
        }

        return new OverlapValidationResult
        {
            HasOverlaps = overlaps.Any(),
            Overlaps = overlaps
        };
    }
}
