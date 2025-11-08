using Microsoft.AspNetCore.Mvc;
using TazaOrda.Domain.DTOs.Files;
using TazaOrda.Domain.Enums;

namespace TazaOrda.API.Controllers;

/// <summary>
/// Контроллер для работы с категориями обращений
/// </summary>
[ApiController]
[Route("api/categories")]
public class CategoriesController(ILogger<CategoriesController> logger) : ControllerBase
{
    /// <summary>
    /// Получить список всех категорий обращений
    /// GET /categories
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CategoryDto>), StatusCodes.Status200OK)]
    public IActionResult GetCategories()
    {
        try
        {
            var categories = Enum.GetValues<ReportCategory>()
                .Select((category, index) => new CategoryDto
                {
                    Id = (int)category,
                    Name = GetCategoryDisplayName(category),
                    Description = GetCategoryDescription(category),
                    IconUrl = GetCategoryIcon(category)
                })
                .ToList();

            return Ok(categories);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting categories");
            return StatusCode(500, new { message = "Произошла ошибка при получении категорий" });
        }
    }

    private static string GetCategoryDisplayName(ReportCategory category)
    {
        return category switch
        {
            ReportCategory.OverflowingBin => "Переполненный бак",
            ReportCategory.DamagedContainer => "Повреждённый контейнер",
            ReportCategory.IllegalDump => "Нелегальная свалка",
            ReportCategory.MissedCollection => "Не вывезен мусор",
            ReportCategory.StreetLitter => "Мусор на улице",
            ReportCategory.SnowIce => "Неубранный снег/лёд",
            ReportCategory.Other => "Другое",
            _ => category.ToString()
        };
    }

    private static string? GetCategoryDescription(ReportCategory category)
    {
        return category switch
        {
            ReportCategory.OverflowingBin => "Контейнер для мусора переполнен",
            ReportCategory.DamagedContainer => "Контейнер повреждён или сломан",
            ReportCategory.IllegalDump => "Несанкционированная свалка мусора",
            ReportCategory.MissedCollection => "Мусор не был вывезен вовремя",
            ReportCategory.StreetLitter => "Мусор в общественных местах",
            ReportCategory.SnowIce => "Необходима уборка снега или льда",
            ReportCategory.Other => "Другие проблемы, связанные с отходами",
            _ => null
        };
    }

    private static string? GetCategoryIcon(ReportCategory category)
    {
        return category switch
        {
            ReportCategory.OverflowingBin => "🗑️",
            ReportCategory.DamagedContainer => "🔨",
            ReportCategory.IllegalDump => "🚫",
            ReportCategory.MissedCollection => "🚛",
            ReportCategory.StreetLitter => "🧹",
            ReportCategory.SnowIce => "❄️",
            ReportCategory.Other => "❓",
            _ => null
        };
    }
}
