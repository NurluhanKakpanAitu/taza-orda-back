using Microsoft.AspNetCore.Mvc;
using TazaOrda.API.Attributes;
using TazaOrda.Domain.DTOs.Users;
using TazaOrda.Domain.Entities;
using TazaOrda.Domain.Interfaces;

namespace TazaOrda.API.Controllers;

/// <summary>
/// Контроллер для работы с профилем пользователя
/// </summary>
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Получить профиль текущего пользователя
    /// GET /users/me
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var user = HttpContext.Items["User"] as User;
        if (user == null)
        {
            return Unauthorized(new { message = "Пользователь не аутентифицирован" });
        }

        var profile = await _userService.GetUserProfileAsync(user.Id, cancellationToken);
        
        if (profile == null)
        {
            return NotFound(new { message = "Профиль не найден" });
        }

        return Ok(profile);
    }

    /// <summary>
    /// Обновить профиль пользователя
    /// PATCH /users/me
    /// </summary>
    [HttpPatch("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var user = HttpContext.Items["User"] as User;
        if (user == null)
        {
            return Unauthorized(new { message = "Пользователь не аутентифицирован" });
        }

        try
        {
            var updatedProfile = await _userService.UpdateUserProfileAsync(user.Id, request, cancellationToken);
            return Ok(updatedProfile);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Profile update failed for user {UserId}", user.Id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for user {UserId}", user.Id);
            return StatusCode(500, new { message = "Произошла ошибка при обновлении профиля" });
        }
    }
}
