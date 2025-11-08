using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TazaOrda.Domain.Entities;

namespace TazaOrda.Infrastructure.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(al => al.Id);

        builder.Property(al => al.Action)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(al => al.EntityType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(al => al.EntityName)
            .HasMaxLength(500);

        builder.Property(al => al.Description)
            .HasMaxLength(2000);

        builder.Property(al => al.OldValues)
            .HasColumnType("jsonb");

        builder.Property(al => al.NewValues)
            .HasColumnType("jsonb");

        builder.Property(al => al.IpAddress)
            .HasMaxLength(50);

        builder.Property(al => al.UserAgent)
            .HasMaxLength(500);

        builder.Property(al => al.Timestamp)
            .IsRequired();

        builder.Property(al => al.Metadata)
            .HasColumnType("jsonb");

        builder.Property(al => al.IsSuccess)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(al => al.ErrorMessage)
            .HasMaxLength(2000);

        builder.Property(al => al.DurationMs);

        builder.Property(al => al.UserName)
            .HasMaxLength(200);

        // Игнорировать вычисляемые свойства
        builder.Ignore(al => al.IsSystemAction);
        builder.Ignore(al => al.IsDataModification);

        // Связи
        builder.HasOne(al => al.User)
            .WithMany()
            .HasForeignKey(al => al.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Индексы
        builder.HasIndex(al => al.Action);
        builder.HasIndex(al => al.EntityType);
        builder.HasIndex(al => al.EntityId);
        builder.HasIndex(al => al.Timestamp);
        builder.HasIndex(al => al.UserId);
        builder.HasIndex(al => new { al.EntityType, al.EntityId });
        builder.HasIndex(al => new { al.UserId, al.Timestamp });

        // Timestamps
        builder.Property(al => al.CreatedAt)
            .IsRequired();

        builder.Property(al => al.UpdatedAt);
    }
}