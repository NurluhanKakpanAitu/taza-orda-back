using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TazaOrda.Domain.Entities;

namespace TazaOrda.Infrastructure.Configurations;

public class RewardRedemptionConfiguration : IEntityTypeConfiguration<RewardRedemption>
{
    public void Configure(EntityTypeBuilder<RewardRedemption> builder)
    {
        builder.ToTable("RewardRedemptions");

        builder.HasKey(rr => rr.Id);

        builder.Property(rr => rr.CoinsSpent)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(rr => rr.RedeemedAt)
            .IsRequired();

        builder.Property(rr => rr.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(rr => rr.Status);

        builder.Property(rr => rr.RedemptionCode)
            .HasMaxLength(100);

        builder.HasIndex(rr => rr.RedemptionCode);

        builder.Property(rr => rr.UserNotes)
            .HasMaxLength(1000);

        builder.Property(rr => rr.AdminNotes)
            .HasMaxLength(1000);

        // Игнорировать вычисляемые свойства
        builder.Ignore(rr => rr.IsPending);
        builder.Ignore(rr => rr.IsApproved);
        builder.Ignore(rr => rr.IsCancelled);
        builder.Ignore(rr => rr.IsDelivered);
        builder.Ignore(rr => rr.IsExpired);

        // Связи
        builder.HasOne(rr => rr.Reward)
            .WithMany(r => r.Redemptions)
            .HasForeignKey(rr => rr.RewardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rr => rr.User)
            .WithMany()
            .HasForeignKey(rr => rr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rr => rr.CoinTransaction)
            .WithOne()
            .HasForeignKey<RewardRedemption>(rr => rr.CoinTransactionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(rr => rr.ProcessedByAdmin)
            .WithMany()
            .HasForeignKey(rr => rr.ProcessedByAdminId)
            .OnDelete(DeleteBehavior.SetNull);

        // Индексы
        builder.HasIndex(rr => new { rr.UserId, rr.RedeemedAt });
        builder.HasIndex(rr => new { rr.RewardId, rr.Status });

        // Timestamps
        builder.Property(rr => rr.CreatedAt)
            .IsRequired();

        builder.Property(rr => rr.UpdatedAt);

        builder.Property(rr => rr.UsedAt);

        builder.Property(rr => rr.ExpiresAt);

        builder.Property(rr => rr.ProcessedAt);
    }
}