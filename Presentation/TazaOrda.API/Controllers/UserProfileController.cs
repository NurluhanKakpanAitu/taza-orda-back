using Microsoft.AspNetCore.Mvc;
using TazaOrda.API.Attributes;
using TazaOrda.Domain.Entities;

namespace TazaOrda.API.Controllers;

/// <summary>
/// Пример контроллера для демонстрации использования аутентификации
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UserProfileController : ControllerBase
{
    private readonly ILogger<UserProfileController> _logger;

    public UserProfileController(ILogger<UserProfileController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Получить профиль текущего пользователя
    /// </summary>
    /// <returns>Информация о пользователе</returns>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetCurrentUser()
    {
        var user = HttpContext.Items["User"] as User;

        if (user == null)
        {
            return Unauthorized(new { message = "Пользователь не аутентифицирован" });
        }

        return Ok(new
        {
            id = user.Id,
            firstName = user.FirstName,
            lastName = user.LastName,
            fullName = user.FullName,
            phoneNumber = user.PhoneNumber,
            email = user.Email,
            role = user.Role.ToString(),
            profilePhotoUrl = user.ProfilePhotoUrl,
            coinBalance = user.CoinBalance,
            activityRating = user.ActivityRating,
            status = user.Status.ToString(),
            isPhoneVerified = user.IsPhoneNumberVerified,
            lastActivity = user.LastActivityDate,
            memberSince = user.CreatedAt
        });
    }

    /// <summary>
    /// Получить баланс монет пользователя
    /// </summary>
    /// <returns>Баланс монет</returns>
    [HttpGet("balance")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetBalance()
    {
        var user = HttpContext.Items["User"] as User;

        if (user == null)
        {
            return Unauthorized(new { message = "Пользователь не аутентифицирован" });
        }

        return Ok(new
        {
            userId = user.Id,
            coinBalance = user.CoinBalance,
            activityRating = user.ActivityRating
        });
    }

    /// <summary>
    /// Пример публичного endpoint (не требует аутентификации)
    /// </summary>
    [HttpGet("public")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetPublicInfo()
    {
        return Ok(new
        {
            message = "Это публичный endpoint, доступный без аутентификации",
            timestamp = DateTime.UtcNow
        });
    }
}
