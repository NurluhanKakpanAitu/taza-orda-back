using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TazaOrda.Domain.Entities;

namespace TazaOrda.Infrastructure.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(300);

        builder.HasIndex(c => c.Name);

        builder.Property(c => c.LegalName)
            .HasMaxLength(500);

        builder.Property(c => c.BusinessIdentificationNumber)
            .HasMaxLength(50);

        builder.HasIndex(c => c.BusinessIdentificationNumber)
            .IsUnique();

        builder.Property(c => c.LegalAddress)
            .HasMaxLength(500);

        builder.Property(c => c.Phone)
            .HasMaxLength(20);

        builder.Property(c => c.Email)
            .HasMaxLength(255);

        builder.Property(c => c.Website)
            .HasMaxLength(500);

        builder.Property(c => c.Description)
            .HasMaxLength(2000);

        builder.Property(c => c.LogoUrl)
            .HasMaxLength(500);

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(c => c.Rating)
            .HasPrecision(3, 2);

        builder.Property(c => c.CompletedReportsCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(c => c.ActiveRoutesCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(c => c.Notes)
            .HasMaxLength(3000);

        // Игнорировать вычисляемые свойства
        builder.Ignore(c => c.EmployeesCount);
        builder.Ignore(c => c.TeamsCount);
        builder.Ignore(c => c.VehiclesCount);
        builder.Ignore(c => c.DistrictsCount);
        builder.Ignore(c => c.IsContractValid);
        builder.Ignore(c => c.IsContractExpiringSoon);

        // Связи
        builder.HasOne(c => c.Director)
            .WithMany()
            .HasForeignKey(c => c.DirectorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.ResponsibleDistricts)
            .WithOne(d => d.Company)
            .HasForeignKey(d => d.CompanyId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.Employees)
            .WithOne(u => u.Company)
            .HasForeignKey(u => u.CompanyId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.Teams)
            .WithOne(t => t.Company)
            .HasForeignKey(t => t.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Vehicles)
            .WithOne(v => v.Company)
            .HasForeignKey(v => v.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Индексы
        builder.HasIndex(c => c.IsActive);
        builder.HasIndex(c => c.Rating);

        // Timestamps
        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt);

        builder.Property(c => c.ContractStartDate);

        builder.Property(c => c.ContractEndDate);
    }
}