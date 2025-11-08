using Microsoft.AspNetCore.Mvc;
using TazaOrda.API.Attributes;
using TazaOrda.Domain.DTOs.Notifications;
using TazaOrda.Domain.Entities;
using TazaOrda.Domain.Interfaces;

namespace TazaOrda.API.Controllers;

/// <summary>
/// Контроллер для работы с уведомлениями
/// </summary>
[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(INotificationService notificationService, ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Получить список уведомлений пользователя
    /// GET /notifications
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(List<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var user = HttpContext.Items["User"] as User;
        if (user == null)
        {
            return Unauthorized(new { message = "Пользователь не аутентифицирован" });
        }

        try
        {
            var notifications = await _notificationService.GetNotificationsAsync(user.Id, unreadOnly, page, pageSize, cancellationToken);
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notifications for user {UserId}", user.Id);
            return StatusCode(500, new { message = "Произошла ошибка при получении уведомлений" });
        }
    }

    /// <summary>
    /// Отметить уведомление как прочитанное
    /// POST /notifications/{id}/read
    /// </summary>
    [HttpPost("{id}/read")]
    [Authorize]
    [ProducesResponseType(typeof(MarkAsReadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
    {
        var user = HttpContext.Items["User"] as User;
        if (user == null)
        {
            return Unauthorized(new { message = "Пользователь не аутентифицирован" });
        }

        try
        {
            var response = await _notificationService.MarkAsReadAsync(id, user.Id, cancellationToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification {NotificationId} as read", id);
            return StatusCode(500, new { message = "Произошла ошибка при обновлении уведомления" });
        }
    }

    /// <summary>
    /// Отметить все уведомления как прочитанные
    /// POST /notifications/read-all
    /// </summary>
    [HttpPost("read-all")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        var user = HttpContext.Items["User"] as User;
        if (user == null)
        {
            return Unauthorized(new { message = "Пользователь не аутентифицирован" });
        }

        try
        {
            await _notificationService.MarkAllAsReadAsync(user.Id, cancellationToken);
            return Ok(new { message = "Все уведомления отмечены как прочитанные" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read for user {UserId}", user.Id);
            return StatusCode(500, new { message = "Произошла ошибка при обновлении уведомлений" });
        }
    }

    /// <summary>
    /// Получить количество непрочитанных уведомлений
    /// GET /notifications/unread-count
    /// </summary>
    [HttpGet("unread-count")]
    [Authorize]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
    {
        var user = HttpContext.Items["User"] as User;
        if (user == null)
        {
            return Unauthorized(new { message = "Пользователь не аутентифицирован" });
        }

        try
        {
            var count = await _notificationService.GetUnreadCountAsync(user.Id, cancellationToken);
            return Ok(new { count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread count for user {UserId}", user.Id);
            return StatusCode(500, new { message = "Произошла ошибка при получении количества уведомлений" });
        }
    }
}
