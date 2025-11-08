using TazaOrda.Domain.Enums;

namespace TazaOrda.Domain.DTOs.Events;

/// <summary>
/// DTO списка событий
/// </summary>
public record EventListDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime StartAt { get; init; }
    public DateTime EndAt { get; init; }
    public Guid? DistrictId { get; init; }
    public string? DistrictName { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public decimal CoinReward { get; init; }
    public string? CoverUrl { get; init; }
    public bool IsActive { get; init; }
    public int ParticipantsCount { get; init; }
    public bool HasStarted { get; init; }
    public bool HasEnded { get; init; }
}

/// <summary>
/// DTO детальной информации о событии
/// </summary>
public record EventDetailDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime StartAt { get; init; }
    public DateTime EndAt { get; init; }
    public Guid? DistrictId { get; init; }
    public string? DistrictName { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public decimal CoinReward { get; init; }
    public string? CoverUrl { get; init; }
    public bool IsActive { get; init; }
    public int ParticipantsCount { get; init; }
    public int CompletedParticipantsCount { get; init; }
    public bool HasStarted { get; init; }
    public bool HasEnded { get; init; }
    public bool IsOngoing { get; init; }
    public bool IsUserJoined { get; init; }
    public EventParticipantStatus? UserParticipantStatus { get; init; }
}

/// <summary>
/// DTO для создания события (admin/operator)
/// </summary>
public record CreateEventRequest
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime StartAt { get; init; }
    public DateTime EndAt { get; init; }
    public Guid? DistrictId { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public decimal CoinReward { get; init; }
    public string? CoverUrl { get; init; }
}

/// <summary>
/// DTO для обновления события
/// </summary>
public record UpdateEventRequest
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public DateTime? StartAt { get; init; }
    public DateTime? EndAt { get; init; }
    public Guid? DistrictId { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public decimal? CoinReward { get; init; }
    public string? CoverUrl { get; init; }
    public bool? IsActive { get; init; }
}

/// <summary>
/// Ответ на присоединение к событию
/// </summary>
public record JoinEventResponse
{
    public Guid ParticipantId { get; init; }
    public string Message { get; init; } = "Вы успешно присоединились к событию";
}

/// <summary>
/// Ответ на чек-ин
/// </summary>
public record CheckInEventResponse
{
    public string Message { get; init; } = "Чек-ин выполнен успешно";
    public DateTime CheckedInAt { get; init; }
}

/// <summary>
/// Запрос на завершение участия (оператор)
/// </summary>
public record CompleteParticipationRequest
{
    public Guid UserId { get; init; }
}

/// <summary>
/// Ответ на завершение участия
/// </summary>
public record CompleteParticipationResponse
{
    public string Message { get; init; } = "Участие завершено, монеты начислены";
    public decimal CoinsAwarded { get; init; }
    public DateTime AwardedAt { get; init; }
}

/// <summary>
/// DTO информации об участнике события
/// </summary>
public record EventParticipantDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public EventParticipantStatus Status { get; init; }
    public DateTime JoinedAt { get; init; }
    public DateTime? CheckedInAt { get; init; }
    public DateTime? CoinsAwardedAt { get; init; }
    public decimal CoinsAwarded { get; init; }
}