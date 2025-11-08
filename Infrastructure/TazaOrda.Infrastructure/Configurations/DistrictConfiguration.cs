using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TazaOrda.Domain.Entities;
using TazaOrda.Domain.ValueObjects;
using System.Text.Json;

namespace TazaOrda.Infrastructure.Configurations;

public class DistrictConfiguration : IEntityTypeConfiguration<District>
{
    public void Configure(EntityTypeBuilder<District> builder)
    {
        builder.ToTable("Districts");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(d => d.Name);

        // Конфигурация Geometry (Polygon) - сохраняем как JSON
        builder.Property(d => d.Geometry)
            .HasConversion(
                polygon => polygon == null ? null : JsonSerializer.Serialize(
                    new PolygonDto 
                    { 
                        Coordinates = polygon.Coordinates.Select(c => 
                            new CoordinateDto { Latitude = c.Latitude, Longitude = c.Longitude }).ToList() 
                    },
                    (JsonSerializerOptions?)null),
                json => json == null ? null : DeserializePolygon(json)
            )
            .HasColumnType("jsonb");

        builder.Property(d => d.Description)
            .HasMaxLength(1000);

        builder.Property(d => d.AreaInSquareKm);

        builder.Property(d => d.PopulationCount);

        builder.Property(d => d.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(d => d.Color)
            .HasMaxLength(20);

        // Игнорировать вычисляемые свойства
        builder.Ignore(d => d.CenterPoint);

        // Связи
        builder.HasOne(d => d.ResponsibleOperator)
            .WithMany()
            .HasForeignKey(d => d.ResponsibleOperatorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(d => d.Reports)
            .WithOne(r => r.District)
            .HasForeignKey(r => r.DistrictId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(d => d.Containers)
            .WithOne(c => c.District)
            .HasForeignKey(c => c.DistrictId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Company)
            .WithMany(c => c.ResponsibleDistricts)
            .HasForeignKey(d => d.CompanyId)
            .OnDelete(DeleteBehavior.SetNull);

        // Timestamps
        builder.Property(d => d.CreatedAt)
            .IsRequired();

        builder.Property(d => d.UpdatedAt);
    }

    private static Polygon? DeserializePolygon(string json)
    {
        try
        {
            var polygonDto = JsonSerializer.Deserialize<PolygonDto>(json, (JsonSerializerOptions?)null);
            if (polygonDto?.Coordinates == null || polygonDto.Coordinates.Count == 0)
                return null;

            var locations = polygonDto.Coordinates
                .Select(c => new Location(c.Latitude, c.Longitude))
                .ToList();

            return new Polygon(locations);
        }
        catch
        {
            // Если не получилось десериализовать, возвращаем null
            return null;
        }
    }

    // DTO для сериализации всего полигона
    public class PolygonDto
    {
        public List<CoordinateDto> Coordinates { get; set; } = new();
    }

    // DTO для сериализации координат
    public class CoordinateDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
