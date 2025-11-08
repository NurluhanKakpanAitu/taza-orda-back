using Microsoft.AspNetCore.Mvc;
using TazaOrda.Domain.DTOs.Auth;
using TazaOrda.Domain.Interfaces;

namespace TazaOrda.API.Controllers;

/// <summary>
/// Контроллер для аутентификации и авторизации
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService, ILogger<AuthController> logger) : ControllerBase
{
    /// <summary>
    /// Регистрация нового пользователя
    /// </summary>
    /// <param name="request">Данные для регистрации</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Токены доступа и информация о пользователе</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var ipAddress = GetIpAddress();
            var response = await authService.RegisterAsync(request, ipAddress, cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Registration failed for phone: {PhoneNumber}", request.PhoneNumber);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during registration for phone: {PhoneNumber}", request.PhoneNumber);
            return StatusCode(500, new { message = "Произошла ошибка при регистрации" });
        }
    }

    /// <summary>
    /// Вход пользователя
    /// </summary>
    /// <param name="request">Данные для входа</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Токены доступа и информация о пользователе</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var ipAddress = GetIpAddress();
            var response = await authService.LoginAsync(request, ipAddress, cancellationToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Login failed for phone: {PhoneNumber}", request.PhoneNumber);
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Login validation failed for phone: {PhoneNumber}", request.PhoneNumber);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during login for phone: {PhoneNumber}", request.PhoneNumber);
            return StatusCode(500, new { message = "Произошла ошибка при входе" });
        }
    }

    /// <summary>
    /// Обновление токена доступа
    /// </summary>
    /// <param name="request">Refresh токен</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Новые токены доступа</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var ipAddress = GetIpAddress();
            var response = await authService.RefreshTokenAsync(request, ipAddress, cancellationToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Token refresh failed");
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new { message = "Произошла ошибка при обновлении токена" });
        }
    }

    /// <summary>
    /// Выход из системы (отзыв refresh токена)
    /// </summary>
    /// <param name="request">Refresh токен</param>
    /// <param name="cancellationToken">Токен отмены</param>
    /// <returns>Результат операции</returns>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var ipAddress = GetIpAddress();
            await authService.RevokeTokenAsync(request.RefreshToken, ipAddress, cancellationToken);
            return Ok(new { message = "Выход выполнен успешно" });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Logout failed");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during logout");
            return StatusCode(500, new { message = "Произошла ошибка при выходе" });
        }
    }

    /// <summary>
    /// Получить IP адрес клиента
    /// </summary>
    private string GetIpAddress()
    {
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
            return Request.Headers["X-Forwarded-For"].ToString();

        return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}
