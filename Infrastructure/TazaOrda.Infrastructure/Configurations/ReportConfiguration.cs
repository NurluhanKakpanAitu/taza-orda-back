using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TazaOrda.Domain.Entities;

namespace TazaOrda.Infrastructure.Configurations;

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("Reports");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(r => r.Category)
            .IsRequired()
            .HasConversion<string>();

        // Конфигурация Location (owned entity)
        builder.OwnsOne(r => r.Location, loc =>
        {
            loc.Property(l => l.Latitude)
                .IsRequired()
                .HasColumnName("Latitude");

            loc.Property(l => l.Longitude)
                .IsRequired()
                .HasColumnName("Longitude");
        });

        builder.Property(r => r.Street)
            .HasMaxLength(300);

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(r => r.Priority)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(r => r.PhotoUrl)
            .HasMaxLength(500);

        builder.Property(r => r.AdminComment)
            .HasMaxLength(1000);

        builder.Property(r => r.LikesCount)
            .IsRequired()
            .HasDefaultValue(0);

        // Игнорировать вычисляемые свойства
        builder.Ignore(r => r.Address);
        builder.Ignore(r => r.IsUrgent);
        builder.Ignore(r => r.IsInProgress);
        builder.Ignore(r => r.IsCompleted);
        builder.Ignore(r => r.IsRejected);
        builder.Ignore(r => r.DaysSinceCreation);

        // Связи
        builder.HasOne(r => r.Author)
            .WithMany(u => u.Reports)
            .HasForeignKey(r => r.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.AssignedTo)
            .WithMany(u => u.AssignedReports)
            .HasForeignKey(r => r.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.District)
            .WithMany(d => d.Reports)
            .HasForeignKey(r => r.DistrictId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Container)
            .WithMany(c => c.Reports)
            .HasForeignKey(r => r.ContainerId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(r => r.Feedbacks)
            .WithOne(f => f.Report)
            .HasForeignKey(f => f.ReportId)
            .OnDelete(DeleteBehavior.Cascade);

        // Индексы
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.Category);
        builder.HasIndex(r => r.Priority);
        builder.HasIndex(r => r.CreatedAt);
        builder.HasIndex(r => new { r.AuthorId, r.Status });

        // Timestamps
        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.UpdatedAt);

        builder.Property(r => r.ClosedAt);
    }
}