using Microsoft.EntityFrameworkCore;
using TazaOrda.Domain.DTOs.Events;
using TazaOrda.Domain.Entities;
using TazaOrda.Domain.Enums;
using TazaOrda.Domain.Interfaces;
using TazaOrda.Infrastructure.Persistence;

namespace TazaOrda.Infrastructure.Services.Events;

/// <summary>
/// Реализация сервиса для работы с акциями/мероприятиями
/// </summary>
public class EventService(TazaOrdaDbContext context, INotificationService notificationService) : IEventService
{
    public async Task<List<EventListDto>> GetEventsAsync(
        Guid? userId = null,
        bool? activeOnly = null,
        Guid? districtId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.Events
            .Include(e => e.District)
            .Include(e => e.Participants)
            .AsQueryable();

        // Фильтрация по активности
        if (activeOnly.HasValue && activeOnly.Value)
        {
            query = query.Where(e => e.IsActive);
        }

        // Фильтрация по району
        if (districtId.HasValue)
        {
            query = query.Where(e => e.DistrictId == districtId.Value);
        }

        // Фильтрация по дате начала
        if (startDate.HasValue)
        {
            query = query.Where(e => e.StartAt >= startDate.Value);
        }

        // Фильтрация по дате окончания
        if (endDate.HasValue)
        {
            query = query.Where(e => e.EndAt <= endDate.Value);
        }

        var events = await query
            .OrderBy(e => e.StartAt)
            .ToListAsync(cancellationToken);

        return events.Select(e => new EventListDto
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            StartAt = e.StartAt,
            EndAt = e.EndAt,
            DistrictId = e.DistrictId,
            DistrictName = e.District?.Name,
            Latitude = e.Latitude,
            Longitude = e.Longitude,
            CoinReward = e.CoinReward,
            CoverUrl = e.CoverUrl,
            IsActive = e.IsActive,
            ParticipantsCount = e.ParticipantsCount,
            HasStarted = e.HasStarted,
            HasEnded = e.HasEnded
        }).ToList();
    }

    public async Task<EventDetailDto?> GetEventByIdAsync(Guid eventId, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var eventEntity = await context.Events
            .Include(e => e.District)
            .Include(e => e.Participants)
            .FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);

        if (eventEntity == null)
        {
            return null;
        }

        var isUserJoined = false;
        EventParticipantStatus? userStatus = null;

        if (userId.HasValue)
        {
            var participant = eventEntity.Participants.FirstOrDefault(p =>
                p.UserId == userId.Value && p.Status != EventParticipantStatus.Cancelled);

            if (participant != null)
            {
                isUserJoined = true;
                userStatus = participant.Status;
            }
        }

        return new EventDetailDto
        {
            Id = eventEntity.Id,
            Title = eventEntity.Title,
            Description = eventEntity.Description,
            StartAt = eventEntity.StartAt,
            EndAt = eventEntity.EndAt,
            DistrictId = eventEntity.DistrictId,
            DistrictName = eventEntity.District?.Name,
            Latitude = eventEntity.Latitude,
            Longitude = eventEntity.Longitude,
            CoinReward = eventEntity.CoinReward,
            CoverUrl = eventEntity.CoverUrl,
            IsActive = eventEntity.IsActive,
            ParticipantsCount = eventEntity.ParticipantsCount,
            CompletedParticipantsCount = eventEntity.CompletedParticipantsCount,
            HasStarted = eventEntity.HasStarted,
            HasEnded = eventEntity.HasEnded,
            IsOngoing = eventEntity.IsOngoing,
            IsUserJoined = isUserJoined,
            UserParticipantStatus = userStatus
        };
    }

    public async Task<EventDetailDto> CreateEventAsync(CreateEventRequest request, User? currentUser, CancellationToken cancellationToken = default)
    {
        // Валидация времени
        if (request.StartAt < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Дата начала события не может быть в прошлом");
        }

        if (request.EndAt <= request.StartAt)
        {
            throw new InvalidOperationException("Дата окончания должна быть позже даты начала");
        }

        // Проверка района если указан
        if (request.DistrictId.HasValue)
        {
            var districtExists = await context.Districts
                .AnyAsync(d => d.Id == request.DistrictId.Value, cancellationToken);

            if (!districtExists)
            {
                throw new InvalidOperationException("Указанный район не найден");
            }
        }

        var eventEntity = new Event
        {
            Title = request.Title,
            Description = request.Description,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            DistrictId = request.DistrictId,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            CoinReward = request.CoinReward,
            CoverUrl = request.CoverUrl,
            IsActive = true
        };

        context.Events.Add(eventEntity);

        // TODO: Add AuditLog when implemented

        await context.SaveChangesAsync(cancellationToken);

        return (await GetEventByIdAsync(eventEntity.Id, null, cancellationToken))!;
    }

    public async Task<EventDetailDto?> UpdateEventAsync(Guid eventId, UpdateEventRequest request, User? currentUser, CancellationToken cancellationToken = default)
    {
        var eventEntity = await context.Events
            .FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);

        if (eventEntity == null)
        {
            return null;
        }

        var changes = new List<string>();

        // Обновление полей
        if (request.Title != null && request.Title != eventEntity.Title)
        {
            changes.Add($"Title: {eventEntity.Title} -> {request.Title}");
            eventEntity.Title = request.Title;
        }

        if (request.Description != null && request.Description != eventEntity.Description)
        {
            changes.Add($"Description изменено");
            eventEntity.Description = request.Description;
        }

        if (request.StartAt.HasValue && request.StartAt.Value != eventEntity.StartAt)
        {
            if (request.StartAt.Value < DateTime.UtcNow)
            {
                throw new InvalidOperationException("Дата начала события не может быть в прошлом");
            }
            changes.Add($"StartAt: {eventEntity.StartAt} -> {request.StartAt.Value}");
            eventEntity.StartAt = request.StartAt.Value;
        }

        if (request.EndAt.HasValue && request.EndAt.Value != eventEntity.EndAt)
        {
            if (request.EndAt.Value <= eventEntity.StartAt)
            {
                throw new InvalidOperationException("Дата окончания должна быть позже даты начала");
            }
            changes.Add($"EndAt: {eventEntity.EndAt} -> {request.EndAt.Value}");
            eventEntity.EndAt = request.EndAt.Value;
        }

        if (request.DistrictId.HasValue && request.DistrictId != eventEntity.DistrictId)
        {
            var districtExists = await context.Districts
                .AnyAsync(d => d.Id == request.DistrictId.Value, cancellationToken);

            if (!districtExists)
            {
                throw new InvalidOperationException("Указанный район не найден");
            }

            changes.Add($"DistrictId: {eventEntity.DistrictId} -> {request.DistrictId}");
            eventEntity.DistrictId = request.DistrictId;
        }

        if (request.Latitude.HasValue && request.Latitude != eventEntity.Latitude)
        {
            changes.Add($"Latitude: {eventEntity.Latitude} -> {request.Latitude}");
            eventEntity.Latitude = request.Latitude;
        }

        if (request.Longitude.HasValue && request.Longitude != eventEntity.Longitude)
        {
            changes.Add($"Longitude: {eventEntity.Longitude} -> {request.Longitude}");
            eventEntity.Longitude = request.Longitude;
        }

        if (request.CoinReward.HasValue && request.CoinReward != eventEntity.CoinReward)
        {
            changes.Add($"CoinReward: {eventEntity.CoinReward} -> {request.CoinReward}");
            eventEntity.CoinReward = request.CoinReward.Value;
        }

        if (request.CoverUrl != null && request.CoverUrl != eventEntity.CoverUrl)
        {
            changes.Add($"CoverUrl изменен");
            eventEntity.CoverUrl = request.CoverUrl;
        }

        if (request.IsActive.HasValue && request.IsActive != eventEntity.IsActive)
        {
            changes.Add($"IsActive: {eventEntity.IsActive} -> {request.IsActive}");
            eventEntity.IsActive = request.IsActive.Value;
        }

        eventEntity.UpdatedAt = DateTime.UtcNow;

        // TODO: Add AuditLog when implemented

        await context.SaveChangesAsync(cancellationToken);

        return await GetEventByIdAsync(eventId, null, cancellationToken);
    }

    public async Task<bool> DeleteEventAsync(Guid eventId, User? currentUser, CancellationToken cancellationToken = default)
    {
        var eventEntity = await context.Events
            .Include(e => e.Participants)
            .FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);

        if (eventEntity == null)
        {
            return false;
        }

        // Проверка - есть ли участники с завершённым участием
        var hasCompletedParticipants = eventEntity.Participants
            .Any(p => p.Status == EventParticipantStatus.Completed);

        if (hasCompletedParticipants)
        {
            throw new InvalidOperationException("Нельзя удалить событие с завершёнными участниками. Деактивируйте событие вместо удаления.");
        }

        // TODO: Add AuditLog when implemented

        context.Events.Remove(eventEntity);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<JoinEventResponse> JoinEventAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default)
    {
        var eventEntity = await context.Events
            .Include(e => e.Participants)
            .FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);

        if (eventEntity == null)
        {
            throw new InvalidOperationException("Событие не найдено");
        }

        if (!eventEntity.IsActive)
        {
            throw new InvalidOperationException("Событие неактивно");
        }

        if (eventEntity.HasEnded)
        {
            throw new InvalidOperationException("Событие уже завершено");
        }

        // Проверить, не присоединился ли пользователь уже
        var existingParticipant = eventEntity.Participants.FirstOrDefault(p =>
            p.UserId == userId && p.Status != EventParticipantStatus.Cancelled);

        if (existingParticipant != null)
        {
            throw new InvalidOperationException("Пользователь уже присоединился к этому событию");
        }

        var participant = new EventParticipant
        {
            EventId = eventId,
            UserId = userId,
            Status = EventParticipantStatus.Joined,
            JoinedAt = DateTime.UtcNow
        };

        context.EventParticipants.Add(participant);
        await context.SaveChangesAsync(cancellationToken);

        // Получить пользователя для уведомления
        var user = await context.Users.FindAsync([userId], cancellationToken);
        if (user != null)
        {
            // Отправить уведомление о присоединении
            await notificationService.SendEventJoinedNotificationAsync(eventEntity, user, cancellationToken);
        }

        return new JoinEventResponse
        {
            ParticipantId = participant.Id,
            Message = "Вы успешно присоединились к событию"
        };
    }

    public async Task<CheckInEventResponse> CheckInEventAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default)
    {
        var participant = await context.EventParticipants
            .Include(p => p.Event)
            .FirstOrDefaultAsync(p => p.EventId == eventId && p.UserId == userId, cancellationToken);

        if (participant == null)
        {
            throw new InvalidOperationException("Вы не зарегистрированы на это событие");
        }

        if (!participant.Event.IsOngoing)
        {
            throw new InvalidOperationException("Чек-ин возможен только во время проведения события");
        }

        participant.CheckIn();

        await context.SaveChangesAsync(cancellationToken);

        return new CheckInEventResponse
        {
            Message = "Чек-ин выполнен успешно",
            CheckedInAt = participant.CheckedInAt!.Value
        };
    }

    public async Task<CompleteParticipationResponse> CompleteParticipationAsync(
        Guid eventId,
        CompleteParticipationRequest request,
        User? currentUser,
        CancellationToken cancellationToken = default)
    {
        var participant = await context.EventParticipants
            .Include(p => p.Event)
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.EventId == eventId && p.UserId == request.UserId, cancellationToken);

        if (participant == null)
        {
            throw new InvalidOperationException("Участник не найден");
        }

        if (participant.Status != EventParticipantStatus.CheckedIn)
        {
            throw new InvalidOperationException("Участник должен выполнить чек-ин перед завершением участия");
        }

        // Создать транзакцию начисления coins
        var transaction = CoinTransaction.CreateCredit(
            user: participant.User,
            amount: participant.Event.CoinReward,
            reason: CoinTransactionReason.EventParticipation,
            description: $"Участие в событии: {participant.Event.Title}"
        );

        transaction.RelatedEventId = eventId;
        transaction.ProcessedByAdminId = currentUser?.Id;
        transaction.ProcessedByAdmin = currentUser;

        context.CoinTransactions.Add(transaction);

        // Обновить баланс пользователя
        participant.User.CoinBalance = transaction.BalanceAfter;

        // Завершить участие
        participant.Complete(participant.Event.CoinReward, transaction);

        // TODO: Add AuditLog when implemented

        await context.SaveChangesAsync(cancellationToken);

        // Отправить уведомление о начислении монет
        await notificationService.SendCoinsAwardedNotificationAsync(
            user: participant.User,
            amount: participant.CoinsAwarded,
            reason: $"Участие в акции \"{participant.Event.Title}\"",
            relatedEntityId: eventId,
            cancellationToken: cancellationToken
        );

        return new CompleteParticipationResponse
        {
            Message = "Участие завершено, монеты начислены",
            CoinsAwarded = participant.CoinsAwarded,
            AwardedAt = participant.CoinsAwardedAt!.Value
        };
    }

    public async Task<List<EventParticipantDto>> GetEventParticipantsAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        var participants = await context.EventParticipants
            .Include(p => p.User)
            .Where(p => p.EventId == eventId && p.Status != EventParticipantStatus.Cancelled)
            .OrderByDescending(p => p.JoinedAt)
            .ToListAsync(cancellationToken);

        return participants.Select(p => new EventParticipantDto
        {
            Id = p.Id,
            UserId = p.UserId,
            UserName = p.User.FullName,
            Status = p.Status,
            JoinedAt = p.JoinedAt,
            CheckedInAt = p.CheckedInAt,
            CoinsAwardedAt = p.CoinsAwardedAt,
            CoinsAwarded = p.CoinsAwarded
        }).ToList();
    }

    public async Task CancelEventParticipationAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default)
    {
        var participant = await context.EventParticipants
            .Include(p => p.Event)
            .FirstOrDefaultAsync(p => p.EventId == eventId && p.UserId == userId, cancellationToken);

        if (participant == null)
        {
            throw new InvalidOperationException("Вы не зарегистрированы на это событие");
        }

        participant.Cancel();

        await context.SaveChangesAsync(cancellationToken);
    }
}