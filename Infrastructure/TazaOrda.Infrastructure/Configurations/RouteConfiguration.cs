using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TazaOrda.Domain.Entities;

namespace TazaOrda.Infrastructure.Configurations;

public class RouteConfiguration : IEntityTypeConfiguration<Route>
{
    public void Configure(EntityTypeBuilder<Route> builder)
    {
        builder.ToTable("Routes");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(r => r.Name);

        builder.Property(r => r.Description)
            .HasMaxLength(1000);

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(r => r.TotalDistanceInKm)
            .HasPrecision(10, 2);

        builder.Property(r => r.EstimatedWasteInTons)
            .HasPrecision(10, 2);

        builder.Property(r => r.ActualWasteInTons)
            .HasPrecision(10, 2);

        builder.Property(r => r.CompletionNotes)
            .HasMaxLength(2000);

        // Игнорировать вычисляемые свойства
        builder.Ignore(r => r.IsActive);
        builder.Ignore(r => r.IsCompleted);
        builder.Ignore(r => r.IsOverdue);
        builder.Ignore(r => r.Duration);
        builder.Ignore(r => r.PointsCount);
        builder.Ignore(r => r.CompletedPointsCount);
        builder.Ignore(r => r.CompletionPercentage);

        // Связи
        builder.HasOne(r => r.Team)
            .WithMany(t => t.Routes)
            .HasForeignKey(r => r.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Vehicle)
            .WithMany(v => v.Routes)
            .HasForeignKey(r => r.VehicleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.District)
            .WithMany()
            .HasForeignKey(r => r.DistrictId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(r => r.RoutePoints)
            .WithOne(rp => rp.Route)
            .HasForeignKey(rp => rp.RouteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Индексы
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.TeamId);
        builder.HasIndex(r => r.ScheduledStartTime);
        builder.HasIndex(r => new { r.TeamId, r.Status });

        // Timestamps
        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.UpdatedAt);

        builder.Property(r => r.ScheduledStartTime)
            .IsRequired();

        builder.Property(r => r.ScheduledEndTime);

        builder.Property(r => r.ActualStartTime);

        builder.Property(r => r.ActualEndTime);
    }
}