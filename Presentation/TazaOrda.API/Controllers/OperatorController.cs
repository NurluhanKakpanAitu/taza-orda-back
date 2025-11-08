using Microsoft.AspNetCore.Mvc;
using TazaOrda.API.Attributes;
using TazaOrda.Domain.DTOs.Operator;
using TazaOrda.Domain.Entities;
using TazaOrda.Domain.Enums;
using TazaOrda.Domain.Interfaces;

namespace TazaOrda.API.Controllers;

/// <summary>
/// Контроллер для операторской панели
/// </summary>
[ApiController]
[Route("api/operator")]
[Authorize]
[RequireRole(UserRole.Operator, UserRole.Admin)]
public class OperatorController(
    IOperatorService operatorService,
    IFileStorageService fileStorageService,
    ILogger<OperatorController> logger)
    : ControllerBase
{
    /// <summary>
    /// Получить список обращений с фильтрацией и пагинацией
    /// GET /operator/reports?status&district_id&category_id&from&to&page&size
    /// </summary>
    [HttpGet("reports")]
    [ProducesResponseType(typeof(PagedOperatorReportsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetReports(
        [FromQuery] ReportStatus? status,
        [FromQuery] Guid? districtId,
        [FromQuery] ReportCategory? category,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var filterParams = new OperatorReportFilterParams
            {
                Status = status,
                DistrictId = districtId,
                Category = category,
                From = from,
                To = to,
                Page = page,
                Size = size,
                SearchTerm = search
            };

            var result = await operatorService.GetReportsAsync(filterParams, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting reports for operator panel");
            return StatusCode(500, new { message = "Произошла ошибка при получении списка обращений" });
        }
    }

    /// <summary>
    /// Получить детали обращения
    /// GET /operator/reports/{id}
    /// </summary>
    [HttpGet("reports/{id:guid}")]
    [ProducesResponseType(typeof(OperatorReportDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetReportById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var report = await operatorService.GetReportByIdAsync(id, cancellationToken);

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
    /// Изменить статус обращения
    /// PATCH /operator/reports/{id}/status
    /// </summary>
    [HttpPatch("reports/{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateReportStatus(
        Guid id,
        [FromBody] UpdateReportStatusRequest request,
        CancellationToken cancellationToken)
    {
        var user = HttpContext.Items["User"] as User;
        if (user == null)
        {
            return Unauthorized(new { message = "Пользователь не аутентифицирован" });
        }

        try
        {
            var success = await operatorService.UpdateReportStatusAsync(
                id,
                user.Id,
                request.Status,
                request.OperatorComment,
                request.PhotoAfterUrl,
                cancellationToken);

            if (!success)
            {
                return NotFound(new { message = "Обращение не найдено" });
            }

            return Ok(new { message = "Статус обращения успешно обновлён" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating report status for report {ReportId}", id);
            return StatusCode(500, new { message = "Произошла ошибка при обновлении статуса" });
        }
    }

    /// <summary>
    /// Загрузить фото "после" для обращения
    /// POST /operator/reports/{id}/photo-after
    /// </summary>
    [HttpPost("reports/{id:guid}/photo-after")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [RequestSizeLimit(5 * 1024 * 1024)] // 5 MB
    public async Task<IActionResult> UploadPhotoAfter(
        Guid id,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        var user = HttpContext.Items["User"] as User;
        if (user == null)
        {
            return Unauthorized(new { message = "Пользователь не аутентифицирован" });
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Файл не может быть пустым" });
        }

        try
        {
            // Загружаем фото в MinIO
            using var stream = file.OpenReadStream();
            var uploadResult = await fileStorageService.UploadFileAsync(
                stream,
                file.FileName,
                file.ContentType,
                file.Length,
                "reports/after",
                cancellationToken);

            // Добавляем URL к обращению
            var success = await operatorService.AddPhotoAfterAsync(id, uploadResult.Url, cancellationToken);

            if (!success)
            {
                // Если обращение не найдено, удаляем загруженный файл
                await fileStorageService.DeleteFileAsync(uploadResult.Path, cancellationToken);
                return NotFound(new { message = "Обращение не найдено" });
            }

            logger.LogInformation("Photo after uploaded for report {ReportId} by operator {OperatorId}", id, user.Id);

            return Ok(new
            {
                message = "Фото успешно загружено",
                photoUrl = uploadResult.Url
            });
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Photo upload validation failed for report {ReportId}", id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error uploading photo after for report {ReportId}", id);
            return StatusCode(500, new { message = "Произошла ошибка при загрузке фото" });
        }
    }

    /// <summary>
    /// Получить статистику за период
    /// GET /operator/stats?period=today|week|month
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(OperatorStatsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStats(
        [FromQuery] string period = "today",
        CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = await operatorService.GetStatsAsync(period, cancellationToken);

            return Ok(stats);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting stats for period {Period}", period);
            return StatusCode(500, new { message = "Произошла ошибка при получении статистики" });
        }
    }

    /// <summary>
    /// Назначить обращение на оператора
    /// POST /operator/reports/{id}/assign
    /// </summary>
    [HttpPost("reports/{id:guid}/assign")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AssignReport(Guid id, CancellationToken cancellationToken)
    {
        var user = HttpContext.Items["User"] as User;
        if (user == null)
        {
            return Unauthorized(new { message = "Пользователь не аутентифицирован" });
        }

        try
        {
            var success = await operatorService.AssignReportAsync(id, user.Id, cancellationToken);

            if (!success)
            {
                return NotFound(new { message = "Обращение не найдено" });
            }

            return Ok(new { message = "Обращение успешно назначено на вас" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error assigning report {ReportId} to operator {OperatorId}", id, user.Id);
            return StatusCode(500, new { message = "Произошла ошибка при назначении обращения" });
        }
    }
}
