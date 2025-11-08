using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TazaOrda.Domain.Entities;

namespace TazaOrda.Infrastructure.Configurations;

public class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("Teams");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(t => t.Name);

        builder.Property(t => t.Description)
            .HasMaxLength(1000);

        builder.Property(t => t.ContactPhone)
            .HasMaxLength(20);

        builder.Property(t => t.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Игнорировать вычисляемые свойства
        builder.Ignore(t => t.MembersCount);
        builder.Ignore(t => t.IsBusy);
        builder.Ignore(t => t.IsAvailable);

        // Связи
        builder.HasOne(t => t.TeamLeader)
            .WithMany()
            .HasForeignKey(t => t.TeamLeaderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(t => t.Members)
            .WithOne(tm => tm.Team)
            .HasForeignKey(tm => tm.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Vehicle)
            .WithOne(v => v.AssignedTeam)
            .HasForeignKey<Team>(t => t.VehicleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.ActiveRoute)
            .WithOne()
            .HasForeignKey<Team>(t => t.ActiveRouteId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(t => t.Routes)
            .WithOne(r => r.Team)
            .HasForeignKey(r => r.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.Company)
            .WithMany(c => c.Teams)
            .HasForeignKey(t => t.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Timestamps
        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.UpdatedAt);
    }
}