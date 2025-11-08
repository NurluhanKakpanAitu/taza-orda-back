using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TazaOrda.Domain.Entities;

namespace TazaOrda.Infrastructure.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.Message)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(n => n.Channel)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(n => n.IsRead)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(n => n.IsSent)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(n => n.SendAttempts)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(n => n.ErrorMessage)
            .HasMaxLength(1000);

        builder.Property(n => n.RelatedEntityType)
            .HasMaxLength(100);

        builder.Property(n => n.ActionUrl)
            .HasMaxLength(500);

        builder.Property(n => n.Metadata)
            .HasColumnType("jsonb");

        builder.Property(n => n.Priority)
            .IsRequired()
            .HasDefaultValue(3);

        // Игнорировать вычисляемые свойства
        builder.Ignore(n => n.IsUnread);
        builder.Ignore(n => n.IsExpired);
        builder.Ignore(n => n.RequiresRetry);

        builder.HasOne(n => n.Sender)
            .WithMany()
            .HasForeignKey(n => n.SenderId)
            .OnDelete(DeleteBehavior.SetNull);

        // Индексы
        builder.HasIndex(n => n.RecipientId);
        builder.HasIndex(n => n.Type);
        builder.HasIndex(n => n.IsRead);
        builder.HasIndex(n => n.IsSent);
        builder.HasIndex(n => new { n.RecipientId, n.IsRead });
        builder.HasIndex(n => new { n.RecipientId, n.CreatedAt });

        // Timestamps
        builder.Property(n => n.CreatedAt)
            .IsRequired();

        builder.Property(n => n.UpdatedAt);

        builder.Property(n => n.ReadAt);

        builder.Property(n => n.SentAt);

        builder.Property(n => n.NextRetryAt);

        builder.Property(n => n.ExpiresAt);
    }
}