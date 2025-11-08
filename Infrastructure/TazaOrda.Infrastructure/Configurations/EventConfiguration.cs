using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TazaOrda.Domain.Entities;

namespace TazaOrda.Infrastructure.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Events");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.HasIndex(e => e.Title);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(3000);

        builder.Property(e => e.StartAt)
            .IsRequired()
            .HasColumnName("start_at");

        builder.Property(e => e.EndAt)
            .IsRequired()
            .HasColumnName("end_at");

        builder.Property(e => e.DistrictId)
            .HasColumnName("district_id");

        builder.Property(e => e.Latitude)
            .HasColumnName("lat");

        builder.Property(e => e.Longitude)
            .HasColumnName("lng");

        builder.Property(e => e.CoinReward)
            .IsRequired()
            .HasPrecision(18, 2)
            .HasColumnName("coin_reward");

        builder.Property(e => e.CoverUrl)
            .HasMaxLength(500)
            .HasColumnName("cover_url");

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true)
            .HasColumnName("is_active");

        // Игнорировать вычисляемые свойства
        builder.Ignore(e => e.ParticipantsCount);
        builder.Ignore(e => e.CompletedParticipantsCount);
        builder.Ignore(e => e.HasStarted);
        builder.Ignore(e => e.HasEnded);
        builder.Ignore(e => e.IsOngoing);
        builder.Ignore(e => e.IsValidTime);
        builder.Ignore(e => e.Duration);

        // Связи
        builder.HasMany(e => e.Participants)
            .WithOne(ep => ep.Event)
            .HasForeignKey(ep => ep.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.District)
            .WithMany()
            .HasForeignKey(e => e.DistrictId)
            .OnDelete(DeleteBehavior.SetNull);

        // Индексы
        builder.HasIndex(e => e.StartAt);
        builder.HasIndex(e => e.IsActive);
        builder.HasIndex(e => e.DistrictId);
        builder.HasIndex(e => new { e.IsActive, e.StartAt });

        // Timestamps
        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.UpdatedAt);
    }
}