using Microsoft.EntityFrameworkCore;
using TazaOrda.Domain.Entities;

namespace TazaOrda.Infrastructure.Persistence;

/// <summary>
/// Контекст базы данных для проекта TazaOrda
/// </summary>
public class TazaOrdaDbContext(DbContextOptions<TazaOrdaDbContext> options) : DbContext(options)
{
    // Пользователи и роли
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    // Обращения и чистота
    public DbSet<Report> Reports { get; set; } = null!;
    public DbSet<District> Districts { get; set; } = null!;
    public DbSet<Container> Containers { get; set; } = null!;

    // Операционная деятельность
    public DbSet<Team> Teams { get; set; } = null!;
    public DbSet<TeamMember> TeamMembers { get; set; } = null!;
    public DbSet<Vehicle> Vehicles { get; set; } = null!;
    public DbSet<Route> Routes { get; set; } = null!;
    public DbSet<RoutePoint> RoutePoints { get; set; } = null!;

    // Геймификация
    public DbSet<CoinTransaction> CoinTransactions { get; set; } = null!;
    public DbSet<Event> Events { get; set; } = null!;
    public DbSet<EventParticipant> EventParticipants { get; set; } = null!;
    public DbSet<Reward> Rewards { get; set; } = null!;
    public DbSet<RewardRedemption> RewardRedemptions { get; set; } = null!;

    // Управление и аналитика
    public DbSet<Company> Companies { get; set; } = null!;
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<Feedback> Feedbacks { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Применяем все конфигурации из текущей сборки
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TazaOrdaDbContext).Assembly);
    }
}