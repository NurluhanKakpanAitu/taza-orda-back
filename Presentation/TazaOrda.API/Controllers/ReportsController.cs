using Microsoft.AspNetCore.Mvc;
using TazaOrda.API.Attributes;
using TazaOrda.Domain.DTOs.Reports;
using TazaOrda.Domain.Entities;
using TazaOrda.Domain.Interfaces;

namespace TazaOrda.API.Controllers;

/// <summary>
/// Контроллер для работы с обращениями
/// </summary>
[ApiController]
[Route("api/reports")]
public class ReportsController(IReportService reportService, ILogger<ReportsController> logger)
    : ControllerBase
{
    /// <summary>
    /// Получить список обращений с фильтрацией
    /// GET /reports?user_id=15 — личные обращения пользователя
    /// GET /reports?status=new — все новые обращения на карте
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ReportListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReports(
        [FromQuery] Guid? user_id = null,
        [FromQuery] string? status = null,
        [FromQuery] Guid? district = null,
        [FromQuery] string? category = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var reports = await reportService.GetReportsAsync(
                user_id, 
                status, 
                district, 
                category, 
                page, 
                pageSize, 
                cancellationToken);

            return Ok(reports);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting reports");
            return StatusCode(500, new { message = "Произошла ошибка при получении обращений" });
        }
    }

    /// <summary>
    /// Получить детальную информацию об обращении
    /// GET /reports/{id}
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ReportDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReport(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var user = HttpContext.Items["User"] as User;
            var report = await reportService.GetReportByIdAsync(id, user?.Id, cancellationToken);

            if (report == null)
            {
                return NotFound(new { message = "Обращение не найдено" });
            }

            return Ok(report);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting report {ReportId}", id);
            return StatusCode(500, new { message = "Произошла ошибка при получении обращения" });
        }
    }

    /// <summary>
    /// Создать новое обращение
    /// POST /reports
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(CreateReportResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateReport([FromBody] CreateReportRequest request, CancellationToken cancellationToken)
    {
        var user = HttpContext.Items["User"] as User;
        if (user == null)
            return Unauthorized(new { message = "Пользователь не аутентифицирован" });

        try
        {
            var response = await reportService.CreateReportAsync(request, user.Id, cancellationToken);
            return CreatedAtAction(nameof(GetReport), new { id = response.ReportId }, response);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Report creation failed for user {UserId}", user.Id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating report for user {UserId}", user.Id);
            return StatusCode(500, new { message = "Произошла ошибка при создании обращения" });
        }
    }

    /// <summary>
    /// Отправить отзыв (оценку) обращения
    /// POST /reports/{id}/feedback
    /// </summary>
    [HttpPost("{id}/feedback")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateFeedback(Guid id, [FromBody] CreateFeedbackRequest request, CancellationToken cancellationToken)
    {
        var user = HttpContext.Items["User"] as User;
        if (user == null)
        {
            return Unauthorized(new { message = "Пользователь не аутентифицирован" });
        }

        try
        {
            await reportService.CreateFeedbackAsync(id, request, user.Id, cancellationToken);
            return Ok(new { message = "Отзыв успешно добавлен" });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Feedback creation failed for report {ReportId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating feedback for report {ReportId}", id);
            return StatusCode(500, new { message = "Произошла ошибка при создании отзыва" });
        }
    }
}
