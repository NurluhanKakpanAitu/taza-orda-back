using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TazaOrda.Domain.Entities;

namespace TazaOrda.Infrastructure.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.LicensePlate)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(v => v.LicensePlate)
            .IsUnique();

        builder.Property(v => v.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(v => v.GpsTrackerId)
            .HasMaxLength(100);

        builder.HasIndex(v => v.GpsTrackerId);

        builder.Property(v => v.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(v => v.Model)
            .HasMaxLength(200);

        builder.Property(v => v.ManufactureYear);

        builder.Property(v => v.CapacityInTons)
            .HasPrecision(10, 2);

        // Конфигурация CurrentLocation (owned entity)
        builder.OwnsOne(v => v.CurrentLocation, loc =>
        {
            loc.Property(l => l.Latitude)
                .HasColumnName("CurrentLatitude");

            loc.Property(l => l.Longitude)
                .HasColumnName("CurrentLongitude");
        });

        builder.Property(v => v.MileageInKm)
            .HasPrecision(12, 2);

        builder.Property(v => v.Notes)
            .HasMaxLength(2000);

        // Игнорировать вычисляемые свойства
        builder.Ignore(v => v.IsAvailable);
        builder.Ignore(v => v.IsOnRoute);
        builder.Ignore(v => v.RequiresMaintenance);

        // Связи
        builder.HasOne(v => v.AssignedTeam)
            .WithOne(t => t.Vehicle)
            .HasForeignKey<Vehicle>(v => v.AssignedTeamId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(v => v.Routes)
            .WithOne(r => r.Vehicle)
            .HasForeignKey(r => r.VehicleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(v => v.Company)
            .WithMany(c => c.Vehicles)
            .HasForeignKey(v => v.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Индексы
        builder.HasIndex(v => v.Status);
        builder.HasIndex(v => v.Type);

        // Timestamps
        builder.Property(v => v.CreatedAt)
            .IsRequired();

        builder.Property(v => v.UpdatedAt);

        builder.Property(v => v.LastLocationUpdate);

        builder.Property(v => v.LastMaintenanceDate);

        builder.Property(v => v.NextMaintenanceDate);
    }
}