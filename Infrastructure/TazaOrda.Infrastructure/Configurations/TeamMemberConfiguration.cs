using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TazaOrda.Domain.Entities;

namespace TazaOrda.Infrastructure.Configurations;

public class TeamMemberConfiguration : IEntityTypeConfiguration<TeamMember>
{
    public void Configure(EntityTypeBuilder<TeamMember> builder)
    {
        builder.ToTable("TeamMembers");

        builder.HasKey(tm => tm.Id);

        builder.Property(tm => tm.Role)
            .HasMaxLength(100);

        builder.Property(tm => tm.JoinedAt)
            .IsRequired();

        builder.Property(tm => tm.LeftAt);

        // Игнорировать вычисляемые свойства
        builder.Ignore(tm => tm.IsActive);

        // Связи
        builder.HasOne(tm => tm.Team)
            .WithMany(t => t.Members)
            .HasForeignKey(tm => tm.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(tm => tm.User)
            .WithMany()
            .HasForeignKey(tm => tm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Индексы
        builder.HasIndex(tm => new { tm.TeamId, tm.UserId });

        // Timestamps
        builder.Property(tm => tm.CreatedAt)
            .IsRequired();

        builder.Property(tm => tm.UpdatedAt);
    }
}