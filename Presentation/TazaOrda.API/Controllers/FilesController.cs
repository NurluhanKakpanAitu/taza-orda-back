using Microsoft.AspNetCore.Mvc;
using TazaOrda.API.Attributes;
using TazaOrda.Domain.DTOs.Files;
using TazaOrda.Domain.Entities;
using TazaOrda.Domain.Interfaces;

namespace TazaOrda.API.Controllers;

/// <summary>
/// Контроллер для работы с файлами
/// </summary>
[ApiController]
[Route("api/files")]
public class FilesController(IFileStorageService fileStorageService, ILogger<FilesController> logger)
    : ControllerBase
{
    /// <summary>
    /// Загрузить фото обращения
    /// POST /files/upload
    /// </summary>
    [HttpPost("upload")]
    [Authorize]
    [ProducesResponseType(typeof(FileUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [RequestSizeLimit(5 * 1024 * 1024)] // 5 MB
    public async Task<IActionResult> UploadFile(IFormFile file, [FromQuery] string folder = "reports", CancellationToken cancellationToken = default)
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
            using var stream = file.OpenReadStream();
            var response = await fileStorageService.UploadFileAsync(
                stream,
                file.FileName,
                file.ContentType,
                file.Length,
                folder,
                cancellationToken);
            
            logger.LogInformation("File uploaded by user {UserId}: {FileName}", user.Id, response.FileName);
            
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "File upload validation failed for user {UserId}", user.Id);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error uploading file for user {UserId}", user.Id);
            return StatusCode(500, new { message = "Произошла ошибка при загрузке файла" });
        }
    }

    /// <summary>
    /// Удалить файл
    /// DELETE /files/{*filePath}
    /// </summary>
    [HttpDelete("{*filePath}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFile(string filePath, CancellationToken cancellationToken)
    {
        var user = HttpContext.Items["User"] as User;
        if (user == null)
        {
            return Unauthorized(new { message = "Пользователь не аутентифицирован" });
        }

        try
        {
            var exists = await fileStorageService.FileExistsAsync(filePath, cancellationToken);
            if (!exists)
            {
                return NotFound(new { message = "Файл не найден" });
            }

            await fileStorageService.DeleteFileAsync(filePath, cancellationToken);
            
            logger.LogInformation("File deleted by user {UserId}: {FilePath}", user.Id, filePath);
            
            return Ok(new { message = "Файл успешно удалён" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting file {FilePath} for user {UserId}", filePath, user.Id);
            return StatusCode(500, new { message = "Произошла ошибка при удалении файла" });
        }
    }
}
