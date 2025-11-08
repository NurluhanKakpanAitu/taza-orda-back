using TazaOrda.Domain.Common;

namespace TazaOrda.Domain.Entities;

/// <summary>
/// Член бригады (связь между User и Team)
/// </summary>
public class TeamMember : BaseEntity
{
    /// <summary>
    /// ID бригады
    /// </summary>
    public Guid TeamId { get; set; }

    /// <summary>
    /// Бригада (навигационное свойство)
    /// </summary>
    public Team Team { get; set; } = null!;

    /// <summary>
    /// ID сотрудника
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Сотрудник (навигационное свойство)
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Роль в бригаде (например: "Водитель", "Грузчик", "Бригадир")
    /// </summary>
    public string? Role { get; set; }

    /// <summary>
    /// Дата вступления в бригаду
    /// </summary>
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Дата выхода из бригады (если применимо)
    /// </summary>
    public DateTime? LeftAt { get; set; }

    /// <summary>
    /// Признак активности в бригаде
    /// </summary>
    public bool IsActive => LeftAt == null;
}