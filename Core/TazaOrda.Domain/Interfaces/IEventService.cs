using TazaOrda.Domain.DTOs.Events;
using TazaOrda.Domain.Entities;

namespace TazaOrda.Domain.Interfaces;

/// <summary>
/// Интерфейс сервиса для работы с акциями/мероприятиями
/// </summary>
public interface IEventService
{
    /// <summary>
    /// Получить список акций с фильтрацией
    /// </summary>
    Task<List<EventListDto>> GetEventsAsync(
        Guid? userId = null,
        bool? activeOnly = null,
        Guid? districtId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить детальную информацию об акции
    /// </summary>
    Task<EventDetailDto?> GetEventByIdAsync(Guid eventId, Guid? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Создать новую акцию (admin/operator)
    /// </summary>
    Task<EventDetailDto> CreateEventAsync(CreateEventRequest request, User? currentUser, CancellationToken cancellationToken = default);

    /// <summary>
    /// Обновить акцию (admin/operator)
    /// </summary>
    Task<EventDetailDto?> UpdateEventAsync(Guid eventId, UpdateEventRequest request, User? currentUser, CancellationToken cancellationToken = default);

    /// <summary>
    /// Удалить акцию (admin/operator)
    /// </summary>
    Task<bool> DeleteEventAsync(Guid eventId, User? currentUser, CancellationToken cancellationToken = default);

    /// <summary>
    /// Присоединиться к акции (resident)
    /// </summary>
    Task<JoinEventResponse> JoinEventAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Выполнить чек-ин на акции (resident)
    /// </summary>
    Task<CheckInEventResponse> CheckInEventAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Завершить участие и начислить монеты (operator)
    /// </summary>
    Task<CompleteParticipationResponse> CompleteParticipationAsync(Guid eventId, CompleteParticipationRequest request, User? currentUser, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить список участников акции (admin/operator)
    /// </summary>
    Task<List<EventParticipantDto>> GetEventParticipantsAsync(Guid eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Отменить регистрацию на акцию (resident)
    /// </summary>
    Task CancelEventParticipationAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default);
}
