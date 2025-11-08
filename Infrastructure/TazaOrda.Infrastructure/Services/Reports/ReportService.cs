using Microsoft.EntityFrameworkCore;
using TazaOrda.Domain.DTOs;
using TazaOrda.Domain.DTOs.Events;
using TazaOrda.Domain.DTOs.Reports;
using TazaOrda.Domain.Entities;
using TazaOrda.Domain.Enums;
using TazaOrda.Domain.Interfaces;
using TazaOrda.Domain.ValueObjects;
using TazaOrda.Infrastructure.Persistence;

namespace TazaOrda.Infrastructure.Services.Reports;

/// <summary>
/// Реализация сервиса для работы с обращениями
/// </summary>
public class ReportService(TazaOrdaDbContext context, INotificationService notificationService) : IReportService
{
    private readonly TazaOrdaDbContext _context = context;
    private readonly INotificationService _notificationService = notificationService;

    public async Task<CreateReportResponse> CreateReportAsync(CreateReportRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException("Пользователь не найден");
        }

        // Парсинг категории
        if (!Enum.TryParse<ReportCategory>(request.Category, true, out var category))
        {
            throw new InvalidOperationException($"Неверная категория обращения: {request.Category}");
        }

        // Определение района по координатам или использование переданного
        Guid districtId;
        if (request.DistrictId.HasValue)
        {
            districtId = request.DistrictId.Value;
        }
        else
        {
            // Берём первый район по умолчанию (в реальном приложении нужна геолокация)
            var defaultDistrict = await _context.Districts.FirstOrDefaultAsync(cancellationToken);
            if (defaultDistrict == null)
            {
                throw new InvalidOperationException("Не найдено ни одного района в системе");
            }
            districtId = defaultDistrict.Id;
        }

        var report = new Report
        {
            AuthorId = userId,
            Category = category,
            Description = request.Description,
            Location = new Location(request.Lat, request.Lng),
            DistrictId = districtId,
            Street = request.Street,
            PhotoUrl = request.PhotoUrl,
            Status = ReportStatus.New,
            Priority = ReportPriority.Medium
        };

        _context.Reports.Add(report);
        
        // Увеличить счётчик обращений пользователя
        user.ReportsCount++;
        
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateReportResponse
        {
            Message = "Report created successfully",
            ReportId = report.Id
        };
    }

    public async Task<List<ReportListDto>> GetReportsAsync(
        Guid? userId = null,
        string? status = null,
        Guid? districtId = null,
        string? category = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Reports
            .Include(r => r.District)
            .Include(r => r.Author)
            .AsQueryable();

        // Фильтр по пользователю
        if (userId.HasValue)
        {
            query = query.Where(r => r.AuthorId == userId.Value);
        }

        // Фильтр по статусу
        if (!string.IsNullOrWhiteSpace(status))
        {
            if (Enum.TryParse<ReportStatus>(status, true, out var reportStatus))
            {
                query = query.Where(r => r.Status == reportStatus);
            }
        }

        // Фильтр по району
        if (districtId.HasValue)
        {
            query = query.Where(r => r.DistrictId == districtId.Value);
        }

        // Фильтр по категории
        if (!string.IsNullOrWhiteSpace(category))
        {
            if (Enum.TryParse<ReportCategory>(category, true, out var reportCategory))
            {
                query = query.Where(r => r.Category == reportCategory);
            }
        }

        var reports = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReportListDto
            {
                Id = r.Id,
                Category = r.Category.ToString(),
                Status = r.Status.ToString(),
                CreatedAt = r.CreatedAt,
                Location = new LocationDto
                {
                    Lat = r.Location.Latitude,
                    Lng = r.Location.Longitude
                },
                PhotoUrl = r.PhotoUrl,
                Description = r.Description,
                Address = r.Address
            })
            .ToListAsync(cancellationToken);

        return reports;
    }

    public async Task<ReportDetailDto?> GetReportByIdAsync(Guid reportId, Guid? userId = null, CancellationToken cancellationToken = default)
    {
        var report = await _context.Reports
            .Include(r => r.Author)
            .Include(r => r.AssignedTo)
            .Include(r => r.District)
            .Include(r => r.Feedbacks)
            .FirstOrDefaultAsync(r => r.Id == reportId, cancellationToken);

        if (report == null)
        {
            return null;
        }

        // Получить отзыв пользователя, если userId передан
        FeedbackDto? userFeedback = null;
        if (userId.HasValue)
        {
            var feedback = report.Feedbacks.FirstOrDefault(f => f.UserId == userId.Value);
            if (feedback != null)
            {
                userFeedback = new FeedbackDto
                {
                    Rating = feedback.Rating,
                    Comment = feedback.Comment,
                    CreatedAt = feedback.CreatedAt
                };
            }
        }

        return new ReportDetailDto
        {
            Id = report.Id,
            Category = report.Category.ToString(),
            Status = report.Status.ToString(),
            Description = report.Description,
            PhotoUrl = report.PhotoUrl,
            CreatedAt = report.CreatedAt,
            UpdatedAt = report.UpdatedAt,
            ClosedAt = report.ClosedAt,
            AdminComment = report.AdminComment,
            Location = new LocationDto
            {
                Lat = report.Location.Latitude,
                Lng = report.Location.Longitude
            },
            Address = report.Address,
            LikesCount = report.LikesCount,
            Author = new AuthorDto
            {
                Id = report.Author.Id,
                Name = report.Author.FullName,
                PhoneNumber = report.Author.PhoneNumber
            },
            AssignedTo = report.AssignedTo != null ? new AssigneeDto
            {
                Id = report.AssignedTo.Id,
                Name = report.AssignedTo.FullName
            } : null,
            UserFeedback = userFeedback
        };
    }

    public async Task CreateFeedbackAsync(Guid reportId, CreateFeedbackRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var report = await _context.Reports
            .Include(r => r.Feedbacks)
            .FirstOrDefaultAsync(r => r.Id == reportId, cancellationToken);

        if (report == null)
        {
            throw new InvalidOperationException("Обращение не найдено");
        }

        if (!report.IsCompleted)
        {
            throw new InvalidOperationException("Нельзя оставить отзыв на незавершённое обращение");
        }

        // Проверить, не оставлял ли пользователь уже отзыв
        var existingFeedback = report.Feedbacks.FirstOrDefault(f => f.UserId == userId);
        if (existingFeedback != null)
        {
            throw new InvalidOperationException("Вы уже оставили отзыв на это обращение");
        }

        var feedback = new Feedback
        {
            ReportId = reportId,
            UserId = userId,
            Rating = request.Rating,
            Comment = request.Comment
        };

        _context.Feedbacks.Add(feedback);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetUserReportsCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Reports
            .Where(r => r.AuthorId == userId)
            .CountAsync(cancellationToken);
    }
}
