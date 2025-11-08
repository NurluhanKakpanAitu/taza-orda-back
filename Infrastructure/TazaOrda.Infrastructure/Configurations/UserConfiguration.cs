using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TazaOrda.Domain.Entities;
using TazaOrda.Domain.Enums;

namespace TazaOrda.Infrastructure.Configurations;

/// <summary>
/// Конфигурация сущности User для Entity Framework
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(u => u.PhoneNumber)
            .IsUnique();

        builder.Property(u => u.Email)
            .HasMaxLength(255);

        builder.HasIndex(u => u.Email);

        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(u => u.ProfilePhotoUrl)
            .HasMaxLength(500);

        builder.Property(u => u.CoinBalance)
            .IsRequired()
            .HasPrecision(18, 2)
            .HasDefaultValue(0);

        builder.Property(u => u.ActivityRating)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(u => u.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(500);

        builder.Property(u => u.IsPhoneNumberVerified)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(u => u.LastActivityDate);

        builder.Property(u => u.ReportsCount)
            .IsRequired()
            .HasDefaultValue(0);

        // Игнорировать вычисляемые свойства
        builder.Ignore(u => u.FullName);
        builder.Ignore(u => u.IsEmployee);
        builder.Ignore(u => u.IsAdmin);
        builder.Ignore(u => u.IsActive);

        // Связи
        builder.HasMany(u => u.Reports)
            .WithOne(r => r.Author)
            .HasForeignKey(r => r.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.AssignedReports)
            .WithOne(r => r.AssignedTo)
            .HasForeignKey(r => r.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(u => u.CoinTransactions)
            .WithOne(ct => ct.User)
            .HasForeignKey(ct => ct.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.Notifications)
            .WithOne(n => n.Recipient)
            .HasForeignKey(n => n.RecipientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.Company)
            .WithMany(c => c.Employees)
            .HasForeignKey(u => u.CompanyId)
            .OnDelete(DeleteBehavior.SetNull);

        // Timestamps
        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.UpdatedAt);
    }
}