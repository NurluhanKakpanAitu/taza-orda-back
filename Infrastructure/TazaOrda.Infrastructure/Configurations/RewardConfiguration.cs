using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TazaOrda.Domain.Entities;

namespace TazaOrda.Infrastructure.Configurations;

public class RewardConfiguration : IEntityTypeConfiguration<Reward>
{
    public void Configure(EntityTypeBuilder<Reward> builder)
    {
        builder.ToTable("Rewards");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(300);

        builder.HasIndex(r => r.Name);

        builder.Property(r => r.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(r => r.CoinsCost)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(r => r.Provider)
            .HasMaxLength(300);

        builder.Property(r => r.Category)
            .HasMaxLength(100);

        builder.HasIndex(r => r.Category);

        builder.Property(r => r.ImageUrl)
            .HasMaxLength(500);

        builder.Property(r => r.TotalQuantity);

        builder.Property(r => r.RedeemedQuantity)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(r => r.IsAvailable)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(r => r.Terms)
            .HasMaxLength(2000);

        builder.Property(r => r.RedemptionInstructions)
            .HasMaxLength(2000);

        builder.Property(r => r.ProviderContact)
            .HasMaxLength(500);

        builder.Property(r => r.MinimumUserLevel);

        builder.Property(r => r.DisplayPriority)
            .IsRequired()
            .HasDefaultValue(0);

        // Игнорировать вычисляемые свойства
        builder.Ignore(r => r.RemainingQuantity);
        builder.Ignore(r => r.IsOutOfStock);
        builder.Ignore(r => r.IsCurrentlyAvailable);
        builder.Ignore(r => r.IsExpired);

        // Связи
        builder.HasMany(r => r.Redemptions)
            .WithOne(rr => rr.Reward)
            .HasForeignKey(rr => rr.RewardId)
            .OnDelete(DeleteBehavior.Cascade);

        // Индексы
        builder.HasIndex(r => r.IsAvailable);
        builder.HasIndex(r => r.CoinsCost);
        builder.HasIndex(r => new { r.IsAvailable, r.DisplayPriority });

        // Timestamps
        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.UpdatedAt);

        builder.Property(r => r.ValidFrom);

        builder.Property(r => r.ValidUntil);
    }
}