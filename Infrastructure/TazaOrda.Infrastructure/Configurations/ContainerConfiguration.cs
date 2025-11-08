using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TazaOrda.Domain.Entities;

namespace TazaOrda.Infrastructure.Configurations;

public class ContainerConfiguration : IEntityTypeConfiguration<Container>
{
    public void Configure(EntityTypeBuilder<Container> builder)
    {
        builder.ToTable("Containers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Address)
            .IsRequired()
            .HasMaxLength(500);

        // Конфигурация Location (owned entity)
        builder.OwnsOne(c => c.Location, loc =>
        {
            loc.Property(l => l.Latitude)
                .IsRequired()
                .HasColumnName("Latitude");

            loc.Property(l => l.Longitude)
                .IsRequired()
                .HasColumnName("Longitude");
        });

        builder.Property(c => c.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(c => c.FillLevel)
            .HasDefaultValue(0);

        builder.Property(c => c.QrCode)
            .HasMaxLength(100);

        builder.HasIndex(c => c.QrCode)
            .IsUnique();

        builder.Property(c => c.IoTSensorId)
            .HasMaxLength(100);

        builder.HasIndex(c => c.IoTSensorId);

        builder.Property(c => c.CapacityInLiters);

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.Condition)
            .HasMaxLength(200);

        builder.Property(c => c.Notes)
            .HasMaxLength(2000);

        builder.Property(c => c.InventoryNumber)
            .HasMaxLength(50);

        builder.HasIndex(c => c.InventoryNumber);

        builder.Property(c => c.ScheduledEmptyingFrequencyInDays);

        // Игнорировать вычисляемые свойства
        builder.Ignore(c => c.HasIoTSensor);
        builder.Ignore(c => c.IsOverfilled);
        builder.Ignore(c => c.RequiresEmptying);
        builder.Ignore(c => c.DaysSinceLastEmptying);
        builder.Ignore(c => c.IsOverdue);

        // Связи
        builder.HasOne(c => c.District)
            .WithMany(d => d.Containers)
            .HasForeignKey(c => c.DistrictId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Reports)
            .WithOne(r => r.Container)
            .HasForeignKey(r => r.ContainerId)
            .OnDelete(DeleteBehavior.SetNull);

        // Индексы
        builder.HasIndex(c => c.Type);
        builder.HasIndex(c => c.DistrictId);
        builder.HasIndex(c => c.IsActive);
        builder.HasIndex(c => new { c.DistrictId, c.Type });

        // Timestamps
        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt);

        builder.Property(c => c.LastEmptiedAt);

        builder.Property(c => c.LastFillLevelUpdate);

        builder.Property(c => c.InstalledAt);
    }
}