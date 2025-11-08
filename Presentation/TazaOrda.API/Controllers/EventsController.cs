using Microsoft.AspNetCore.Mvc;
using TazaOrda.API.Attributes;
using TazaOrda.Domain.DTOs.Events;
using TazaOrda.Domain.Entities;
using TazaOrda.Domain.Enums;
using TazaOrda.Domain.Interfaces;

namespace TazaOrda.API.Controllers;

/// <summary>
/// Контроллер для работы с акциями/мероприятиями
/// </summary>
[ApiController]
[Route("api/events")]
public class EventsController(IEventService eventService, ILogger<EventsController> logger) : ControllerBase
{
    /// <summary>
    /// Получить список акций с фильтрацией
    /// GET /api/events
    /// </summary>
    /// <param name="activeOnly">Только активные события</param>
    /// <param name="districtId">Фильтр по району</param>
    /// <param name="startDate">Фильтр по дате начала (от)</param>
    /// <param name="endDate">Фильтр по дате окончания (до)</param>
    /// <param name="cancellationToken">Токен отмены</param>
    [HttpGet]
    [ProducesResponseType(typeof(List<EventListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEvents(
        [FromQuery] bool? activeOnly = null,
        [FromQuery] Guid? districtId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = HttpContext.Items["User"] as User;
            var events = await eventService.GetEventsAsync(
                userId: user?.Id,
                activeOnly: activeOnly,
                districtId: districtId,
                startDate: startDate,
                endDate: endDate,
                cancellationToken: cancellationToken);

            return Ok(events);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting events");
            return StatusCode(500, new { message = "Произошла ошибка при получении списка акций" });
        }
    }

    /// <summary>
    /// Получить детальную информацию об акции
    /// GET /api/events/{id}
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EventDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEvent(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var user = HttpContext.Items["User"] as User;
            var eventDetail = await eventService.GetEventByIdAsync(id, user?.Id, cancellationToken);

            if (eventDetail == null)
            {
                return NotFound(new { message = "Акция не найдена" });
            }

            return Ok(eventDetail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting event {EventId}", id);
            return StatusCode(500, new { message = "Произошла ошибка при получении акции" });
        }
    }

    /// <summary>
    /// Создать новую акцию (admin/operator)
    /// POST /api/events
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(EventDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequest request, CancellationToken cancellationToken)
    {
        var user = HttpContext.Items["User"] as User;
        if (user == null)
        {
            return Unauthorized(new { message = "Пользователь не аутентифицирован" });
        }

        try
        {
            var eventDetail = await eventService.CreateEventAsync(request, user, cancellationToken);
            return CreatedAtAction(nameof(GetEvent), new { id = eventDetail.Id }, eventDetail);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Event creation failed");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating event");
            return StatusCode(500, new { message = "Произошла ошибка при создании акции" });
        }
    }

    /// <summary>
    /// Обновить акцию (admin/operator)
    /// PATCH /api/events/{id}
    /// </summary>
    [HttpPatch("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(EventDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEvent(Guid id, [FromBody] UpdateEventRequest request, CancellationToken cancellationToken)
    {
        var user = HttpContext.Items["User"] as User;
        if (user == null)
        {
            return Unauthorized(new { message = "Пользователь не аутентифицирован" });
        }

        try
        {
            var eventDetail = await eventService.UpdateEventAsync(id, request, user, cancellationToken);

            if (eventDetail == null)
            {
                return NotFound(new { message = "Акция не найдена" });
            }

            return Ok(eventDetail);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Event update failed for event {EventId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating event {EventId}", id);
            return StatusCode(500, new { message = "Произошла ошибка при обновлении акции" });
        }
    }

    /// <summary>
    /// Удалить акцию (admin/operator)
    /// DELETE /api/events/{id}
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEvent(Guid id, CancellationToken cancellationToken)
    {
        var user = HttpContext.Items["User"] as User;
        if (user == null)
        {
            return Unauthorized(new { message = "Пользователь не аутентифицирован" });
        }

        try
        {
            var deleted = await eventService.DeleteEventAsync(id, user, cancellationToken);

            if (!deleted)
            {
                return NotFound(new { message = "Акция не найдена" });
            }

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Event deletion failed for event {EventId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting event {EventId}", id);
            return StatusCode(500, new { message = "Произошла ошибка при удалении акции" });
        }
    }

    /// <summary>
    /// Присоединиться к акции (resident)
    /// POST /api/events/{id}/join
    /// </summary>
    [HttpPost("{id}/join")]
    [ProducesResponseType(typeof(JoinEventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> JoinEvent(Guid id, CancellationToken cancellationToken)
    {
        var user = HttpContext.Items["User"] as User;
        if (user == null)
        {
            return Unauthorized(new { message = "Пользователь не аутентифицирован" });
        }

        try
        {
            var response = await eventService.JoinEventAsync(id, user.Id, cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Event join failed for event {EventId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error joining event {EventId}", id);
            return StatusCode(500, new { message = "Произошла ошибка при регистрации на акцию" });
        }
    }

    /// <summary>
    /// Выполнить чек-ин на акции (resident)
    /// POST /api/events/{id}/check-in
    /// </summary>
    [HttpPost("{id}/check-in")]
    [ProducesResponseType(typeof(CheckInEventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CheckInEvent(Guid id, CancellationToken cancellationToken)
    {
        var user = HttpContext.Items["User"] as User;
        if (user == null)
        {
            return Unauthorized(new { message = "Пользователь не аутентифицирован" });
        }

        try
        {
            var response = await eventService.CheckInEventAsync(id, user.Id, cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Event check-in failed for event {EventId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking in to event {EventId}", id);
            return StatusCode(500, new { message = "Произошла ошибка при чек-ине на акцию" });
        }
    }

    /// <summary>
    /// Завершить участие и начислить монеты (operator)
    /// POST /api/events/{id}/complete
    /// </summary>
    [HttpPost("{id}/complete")]
    [ProducesResponseType(typeof(CompleteParticipationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CompleteParticipation(
        Guid id,
        [FromBody] CompleteParticipationRequest request,
        CancellationToken cancellationToken)
    {
        var user = HttpContext.Items["User"] as User;
        if (user == null)
        {
            return Unauthorized(new { message = "Пользователь не аутентифицирован" });
        }

        try
        {
            var response = await eventService.CompleteParticipationAsync(id, request, user, cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Event participation completion failed for event {EventId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error completing participation in event {EventId}", id);
            return StatusCode(500, new { message = "Произошла ошибка при завершении участия" });
        }
    }

    /// <summary>
    /// Получить список участников акции (admin/operator)
    /// GET /api/events/{id}/participants
    /// </summary>
    [HttpGet("{id}/participants")]
    [ProducesResponseType(typeof(List<EventParticipantDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEventParticipants(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var participants = await eventService.GetEventParticipantsAsync(id, cancellationToken);
            return Ok(participants);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting participants for event {EventId}", id);
            return StatusCode(500, new { message = "Произошла ошибка при получении списка участников" });
        }
    }

    /// <summary>
    /// Отменить регистрацию на акцию (resident)
    /// POST /api/events/{id}/cancel
    /// </summary>
    [HttpPost("{id}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CancelEventParticipation(Guid id, CancellationToken cancellationToken)
    {
        var user = HttpContext.Items["User"] as User;
        if (user == null)
        {
            return Unauthorized(new { message = "Пользователь не аутентифицирован" });
        }

        try
        {
            await eventService.CancelEventParticipationAsync(id, user.Id, cancellationToken);
            return Ok(new { message = "Регистрация отменена" });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Event cancellation failed for event {EventId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error cancelling event {EventId}", id);
            return StatusCode(500, new { message = "Произошла ошибка при отмене регистрации" });
        }
    }
}