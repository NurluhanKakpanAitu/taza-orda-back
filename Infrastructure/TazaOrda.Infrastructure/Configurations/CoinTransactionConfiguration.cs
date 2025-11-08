using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TazaOrda.Domain.Entities;

namespace TazaOrda.Infrastructure.Configurations;

public class CoinTransactionConfiguration : IEntityTypeConfiguration<CoinTransaction>
{
    public void Configure(EntityTypeBuilder<CoinTransaction> builder)
    {
        builder.ToTable("CoinTransactions");

        builder.HasKey(ct => ct.Id);

        builder.Property(ct => ct.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(ct => ct.Reason)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(ct => ct.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(ct => ct.BalanceAfter)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(ct => ct.Description)
            .HasMaxLength(1000);

        builder.Property(ct => ct.IsReversed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(ct => ct.ProcessedAt)
            .IsRequired();

        // Игнорировать вычисляемые свойства
        builder.Ignore(ct => ct.IsCredit);
        builder.Ignore(ct => ct.IsDebit);
        builder.Ignore(ct => ct.SignedAmount);

        // Связи
        builder.HasOne(ct => ct.User)
            .WithMany(u => u.CoinTransactions)
            .HasForeignKey(ct => ct.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ct => ct.RelatedReport)
            .WithMany()
            .HasForeignKey(ct => ct.RelatedReportId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(ct => ct.RelatedEvent)
            .WithMany()
            .HasForeignKey(ct => ct.RelatedEventId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(ct => ct.RelatedReward)
            .WithMany()
            .HasForeignKey(ct => ct.RelatedRewardId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(ct => ct.ProcessedByAdmin)
            .WithMany()
            .HasForeignKey(ct => ct.ProcessedByAdminId)
            .OnDelete(DeleteBehavior.SetNull);

        // Индексы
        builder.HasIndex(ct => ct.UserId);
        builder.HasIndex(ct => ct.Type);
        builder.HasIndex(ct => ct.Reason);
        builder.HasIndex(ct => ct.ProcessedAt);
        builder.HasIndex(ct => new { ct.UserId, ct.ProcessedAt });

        // Timestamps
        builder.Property(ct => ct.CreatedAt)
            .IsRequired();

        builder.Property(ct => ct.UpdatedAt);
    }
}