using Microsoft.AspNetCore.Mvc;
using TazaOrda.API.Attributes;
using TazaOrda.Domain.DTOs.Coins;
using TazaOrda.Domain.Entities;
using TazaOrda.Domain.Interfaces;

namespace TazaOrda.API.Controllers;

/// <summary>
/// Контроллер для работы с coins (внутренней валютой)
/// </summary>
[ApiController]
[Route("api/coins")]
public class CoinsController : ControllerBase
{
    private readonly ICoinService _coinService;
    private readonly ILogger<CoinsController> _logger;

    public CoinsController(ICoinService coinService, ILogger<CoinsController> logger)
    {
        _coinService = coinService;
        _logger = logger;
    }

    /// <summary>
    /// Получить историю транзакций с coins
    /// GET /coins/history
    /// </summary>
    [HttpGet("history")]
    [Authorize]
    [ProducesResponseType(typeof(List<CoinHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCoinHistory(
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
            var history = await _coinService.GetCoinHistoryAsync(user.Id, page, pageSize, cancellationToken);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting coin history for user {UserId}", user.Id);
            return StatusCode(500, new { message = "Произошла ошибка при получении истории" });
        }
    }

    /// <summary>
    /// Получить баланс пользователя
    /// GET /coins/balance
    /// </summary>
    [HttpGet("balance")]
    [Authorize]
    [ProducesResponseType(typeof(CoinBalanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBalance(CancellationToken cancellationToken)
    {
        var user = HttpContext.Items["User"] as User;
        if (user == null)
        {
            return Unauthorized(new { message = "Пользователь не аутентифицирован" });
        }

        try
        {
            var balance = await _coinService.GetCoinBalanceAsync(user.Id, cancellationToken);
            return Ok(balance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting coin balance for user {UserId}", user.Id);
            return StatusCode(500, new { message = "Произошла ошибка при получении баланса" });
        }
    }
}
