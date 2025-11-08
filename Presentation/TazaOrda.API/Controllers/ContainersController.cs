using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TazaOrda.Domain.Entities;
using TazaOrda.Infrastructure.Persistence;

namespace TazaOrda.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContainersController(TazaOrdaDbContext context, ILogger<ContainersController> logger)
    : ControllerBase
{
    /// <summary>
    /// Получить все контейнеры
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Container>>> GetContainers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var containers = await context.Containers
            .Include(c => c.District)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(containers);
    }

    /// <summary>
    /// Получить контейнер по ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Container>> GetContainer(Guid id)
    {
        var container = await context.Containers
            .Include(c => c.District)
            .Include(c => c.Reports)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (container == null)
        {
            return NotFound();
        }

        return Ok(container);
    }

    /// <summary>
    /// Создать новый контейнер
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Container>> CreateContainer(Container container)
    {
        container.CreatedAt = DateTime.UtcNow;

        context.Containers.Add(container);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetContainer), new { id = container.Id }, container);
    }

    /// <summary>
    /// Обновить контейнер
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateContainer(Guid id, Container container)
    {
        if (id != container.Id)
        {
            return BadRequest();
        }

        container.UpdatedAt = DateTime.UtcNow;
        context.Entry(container).State = EntityState.Modified;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await ContainerExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    /// <summary>
    /// Удалить контейнер
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteContainer(Guid id)
    {
        var container = await context.Containers.FindAsync(id);
        if (container == null)
        {
            return NotFound();
        }

        context.Containers.Remove(container);
        await context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Обновить уровень заполненности контейнера
    /// </summary>
    [HttpPost("{id}/update-fill-level")]
    public async Task<IActionResult> UpdateFillLevel(Guid id, [FromBody] int fillLevel)
    {
        var container = await context.Containers.FindAsync(id);
        if (container == null)
        {
            return NotFound();
        }

        container.UpdateFillLevel(fillLevel);
        await context.SaveChangesAsync();

        return Ok(container);
    }

    /// <summary>
    /// Опорожнить контейнер
    /// </summary>
    [HttpPost("{id}/empty")]
    public async Task<IActionResult> EmptyContainer(Guid id)
    {
        var container = await context.Containers.FindAsync(id);
        if (container == null)
        {
            return NotFound();
        }

        container.Empty();
        await context.SaveChangesAsync();

        return Ok(container);
    }

    /// <summary>
    /// Отметить контейнер как требующий ремонта
    /// </summary>
    [HttpPost("{id}/mark-for-repair")]
    public async Task<IActionResult> MarkForRepair(Guid id, [FromBody] string reason)
    {
        var container = await context.Containers.FindAsync(id);
        if (container == null)
        {
            return NotFound();
        }

        container.MarkForRepair(reason);
        await context.SaveChangesAsync();

        return Ok(container);
    }

    /// <summary>
    /// Получить контейнеры, требующие опорожнения
    /// </summary>
    [HttpGet("requiring-emptying")]
    public async Task<ActionResult<IEnumerable<Container>>> GetContainersRequiringEmptying()
    {
        var containers = await context.Containers
            .Include(c => c.District)
            .ToListAsync();

        var requiring = containers.Where(c => c.RequiresEmptying).ToList();

        return Ok(requiring);
    }

    private async Task<bool> ContainerExists(Guid id)
    {
        return await context.Containers.AnyAsync(e => e.Id == id);
    }
}