using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TazaOrda.Domain.Entities;

namespace TazaOrda.Infrastructure.Configurations;

public class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
{
    public void Configure(EntityTypeBuilder<Feedback> builder)
    {
        builder.ToTable("Feedbacks");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Rating)
            .IsRequired();

        builder.Property(f => f.Comment)
            .HasMaxLength(2000);

        builder.Property(f => f.Category)
            .HasMaxLength(100);

        builder.Property(f => f.PhotoUrls)
            .HasMaxLength(2000);

        builder.Property(f => f.IsPublic)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(f => f.IsApproved)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(f => f.ModeratorComment)
            .HasMaxLength(1000);

        builder.Property(f => f.Response)
            .HasMaxLength(2000);

        builder.Property(f => f.HelpfulCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(f => f.IsRejected)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(f => f.RejectionReason)
            .HasMaxLength(1000);

        // Игнорировать вычисляемые свойства
        builder.Ignore(f => f.IsPositive);
        builder.Ignore(f => f.IsNegative);
        builder.Ignore(f => f.HasResponse);
        builder.Ignore(f => f.IsPendingModeration);

        // Связи
        builder.HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Report)
            .WithMany(r => r.Feedbacks)
            .HasForeignKey(f => f.ReportId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Company)
            .WithMany()
            .HasForeignKey(f => f.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Event)
            .WithMany()
            .HasForeignKey(f => f.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.ModeratedBy)
            .WithMany()
            .HasForeignKey(f => f.ModeratedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(f => f.RespondedBy)
            .WithMany()
            .HasForeignKey(f => f.RespondedById)
            .OnDelete(DeleteBehavior.SetNull);

        // Индексы
        builder.HasIndex(f => f.Rating);
        builder.HasIndex(f => f.IsPublic);
        builder.HasIndex(f => f.IsApproved);
        builder.HasIndex(f => f.ReportId);
        builder.HasIndex(f => f.CompanyId);
        builder.HasIndex(f => new { f.UserId, f.CreatedAt });

        // Timestamps
        builder.Property(f => f.CreatedAt)
            .IsRequired();

        builder.Property(f => f.UpdatedAt);

        builder.Property(f => f.ModeratedAt);

        builder.Property(f => f.RespondedAt);
    }
}