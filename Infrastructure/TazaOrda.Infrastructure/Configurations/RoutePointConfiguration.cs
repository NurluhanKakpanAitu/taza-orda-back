using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TazaOrda.Domain.Entities;

namespace TazaOrda.Infrastructure.Configurations;

public class RoutePointConfiguration : IEntityTypeConfiguration<RoutePoint>
{
    public void Configure(EntityTypeBuilder<RoutePoint> builder)
    {
        builder.ToTable("RoutePoints");

        builder.HasKey(rp => rp.Id);

        builder.Property(rp => rp.SequenceNumber)
            .IsRequired();

        // Конфигурация Location (owned entity)
        builder.OwnsOne(rp => rp.Location, loc =>
        {
            loc.Property(l => l.Latitude)
                .IsRequired()
                .HasColumnName("Latitude");

            loc.Property(l => l.Longitude)
                .IsRequired()
                .HasColumnName("Longitude");
        });

        builder.Property(rp => rp.Address)
            .HasMaxLength(500);

        builder.Property(rp => rp.Description)
            .HasMaxLength(1000);

        builder.Property(rp => rp.IsCompleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(rp => rp.Notes)
            .HasMaxLength(1000);

        builder.Property(rp => rp.PhotoUrl)
            .HasMaxLength(500);

        builder.Property(rp => rp.CollectedWasteInKg)
            .HasPrecision(10, 2);

        // Игнорировать вычисляемые свойства
        builder.Ignore(rp => rp.IsOverdue);

        // Связи
        builder.HasOne(rp => rp.Route)
            .WithMany(r => r.RoutePoints)
            .HasForeignKey(rp => rp.RouteId)
            .OnDelete(DeleteBehavior.Cascade);

        // Индексы
        builder.HasIndex(rp => new { rp.RouteId, rp.SequenceNumber });
        builder.HasIndex(rp => rp.IsCompleted);

        // Timestamps
        builder.Property(rp => rp.CreatedAt)
            .IsRequired();

        builder.Property(rp => rp.UpdatedAt);

        builder.Property(rp => rp.ScheduledTime);

        builder.Property(rp => rp.ActualTime);
    }
}