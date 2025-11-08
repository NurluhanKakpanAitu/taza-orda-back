using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TazaOrda.Domain.Entities;

namespace TazaOrda.Infrastructure.Configurations;

public class EventParticipantConfiguration : IEntityTypeConfiguration<EventParticipant>
{
    public void Configure(EntityTypeBuilder<EventParticipant> builder)
    {
        builder.ToTable("EventParticipants");

        builder.HasKey(ep => ep.Id);

        builder.Property(ep => ep.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasColumnName("status");

        builder.Property(ep => ep.JoinedAt)
            .IsRequired()
            .HasColumnName("joined_at");

        builder.Property(ep => ep.CheckedInAt)
            .HasColumnName("checked_in_at");

        builder.Property(ep => ep.CoinsAwardedAt)
            .HasColumnName("coins_awarded_at");

        builder.Property(ep => ep.CoinsAwarded)
            .IsRequired()
            .HasPrecision(18, 2)
            .HasDefaultValue(0)
            .HasColumnName("coins_awarded");

        builder.Property(ep => ep.CoinTransactionId)
            .HasColumnName("coin_transaction_id");

        // Игнорировать вычисляемые свойства
        builder.Ignore(ep => ep.IsCheckedIn);
        builder.Ignore(ep => ep.IsCompleted);
        builder.Ignore(ep => ep.IsCancelled);

        // Связи
        builder.HasOne(ep => ep.User)
            .WithMany()
            .HasForeignKey(ep => ep.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ep => ep.Event)
            .WithMany(e => e.Participants)
            .HasForeignKey(ep => ep.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ep => ep.CoinTransaction)
            .WithOne()
            .HasForeignKey<EventParticipant>(ep => ep.CoinTransactionId)
            .OnDelete(DeleteBehavior.SetNull);

        // Индексы
        builder.HasIndex(ep => new { ep.EventId, ep.UserId })
            .IsUnique();
        builder.HasIndex(ep => ep.Status);
        builder.HasIndex(ep => ep.JoinedAt);

        // Timestamps
        builder.Property(ep => ep.CreatedAt)
            .IsRequired();

        builder.Property(ep => ep.UpdatedAt);
    }
}