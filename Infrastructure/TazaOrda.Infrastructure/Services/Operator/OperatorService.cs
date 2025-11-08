using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TazaOrda.Domain.DTOs.Operator;
using TazaOrda.Domain.Entities;
using TazaOrda.Domain.Enums;
using TazaOrda.Domain.Interfaces;
using TazaOrda.Infrastructure.Persistence;

namespace TazaOrda.Infrastructure.Services.Operator;

/// <summary>
/// Реализация сервиса для операторской панели
/// </summary>
public class OperatorService(TazaOrdaDbContext context, ILogger<OperatorService> logger) : IOperatorService
{
    public async Task<PagedOperatorReportsResponse> GetReportsAsync(
        OperatorReportFilterParams filterParams,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = context.Reports
                .Include(r => r.Author)
                .Include(r => r.District)
                .AsQueryable();

            // Применяем фильтры
            if (filterParams.Status.HasValue)
            {
                query = query.Where(r => r.Status == filterParams.Status.Value);
            }

            if (filterParams.DistrictId.HasValue)
            {
                query = query.Where(r => r.DistrictId == filterParams.DistrictId.Value);
            }

            if (filterParams.Category.HasValue)
            {
                query = query.Where(r => r.Category == filterParams.Category.Value);
            }

            if (filterParams.From.HasValue)
            {
                query = query.Where(r => r.CreatedAt >= filterParams.From.Value);
            }

            if (filterParams.To.HasValue)
            {
                query = query.Where(r => r.CreatedAt <= filterParams.To.Value);
            }

            if (!string.IsNullOrWhiteSpace(filterParams.SearchTerm))
            {
                var searchLower = filterParams.SearchTerm.ToLower();
                query = query.Where(r =>
                    r.Description.ToLower().Contains(searchLower) ||
                    (r.Street != null && r.Street.ToLower().Contains(searchLower)) ||
                    r.Author.FirstName.ToLower().Contains(searchLower) ||
                    r.Author.LastName.ToLower().Contains(searchLower));
            }

            // Подсчёт общего количества
            var total = await query.CountAsync(cancellationToken);

            // Сортировка (новые сначала)
            query = query.OrderByDescending(r => r.CreatedAt);

            // Пагинация
            var page = filterParams.Page < 1 ? 1 : filterParams.Page;
            var size = filterParams.Size < 1 ? 20 : filterParams.Size;
            
            var reports = await query
                .Skip((page - 1) * size)
                .Take(size)
                .Select(r => new OperatorReportListDto
                {
                    Id = r.Id,
                    Category = r.Category,
                    CategoryName = GetCategoryNameStatic(r.Category),
                    Status = r.Status,
                    Description = r.Description,
                    Latitude = r.Location.Latitude,
                    Longitude = r.Location.Longitude,
                    Street = r.Street,
                    CreatedAt = r.CreatedAt,
                    UserName = r.Author.FirstName + " " + r.Author.LastName,
                    DistrictName = r.District.Name,
                    HasPhotoBefore = !string.IsNullOrEmpty(r.PhotoUrl),
                    HasPhotoAfter = false
                })
                .ToListAsync(cancellationToken);

            var totalPages = (int)Math.Ceiling(total / (double)size);

            return new PagedOperatorReportsResponse
            {
                Reports = reports,
                Total = total,
                Page = page,
                Size = size,
                TotalPages = totalPages
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting reports for operator panel");
            throw;
        }
    }

    public async Task<OperatorReportDetailDto?> GetReportByIdAsync(Guid reportId, CancellationToken cancellationToken = default)
    {
        try
        {
            var report = await context.Reports
                .Include(r => r.Author)
                .Include(r => r.District)
                .Include(r => r.AssignedTo)
                .Include(r => r.Feedbacks)
                .FirstOrDefaultAsync(r => r.Id == reportId, cancellationToken);

            if (report == null)
            {
                return null;
            }

            var feedback = report.Feedbacks.FirstOrDefault();

            return new OperatorReportDetailDto
            {
                Id = report.Id,
                Category = report.Category,
                CategoryName = GetCategoryNameStatic(report.Category),
                Status = report.Status,
                Description = report.Description,
                Latitude = report.Location.Latitude,
                Longitude = report.Location.Longitude,
                Street = report.Street,
                PhotoBefore = report.PhotoUrl,
                PhotoAfter = null,
                CreatedAt = report.CreatedAt,
                UpdatedAt = report.UpdatedAt,
                CompletedAt = report.ClosedAt,
                OperatorComment = report.AdminComment,
                Rating = feedback?.Rating,
                UserFeedback = feedback?.Comment,
                UserId = report.AuthorId,
                UserName = report.Author.FirstName + " " + report.Author.LastName,
                UserPhone = report.Author.PhoneNumber,
                DistrictId = report.DistrictId,
                DistrictName = report.District?.Name,
                AssignedOperatorId = report.AssignedToId,
                AssignedOperatorName = report.AssignedTo != null ? report.AssignedTo.FirstName + " " + report.AssignedTo.LastName : null
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting report details for report {ReportId}", reportId);
            throw;
        }
    }

    public async Task<bool> UpdateReportStatusAsync(
        Guid reportId,
        Guid operatorId,
        ReportStatus status,
        string? comment,
        string? photoAfterUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var report = await context.Reports.FindAsync([reportId], cancellationToken);
            
            if (report == null)
            {
                logger.LogWarning("Report {ReportId} not found", reportId);
                return false;
            }

            var oldStatus = report.Status;
            report.Status = status;
            report.AdminComment = comment;
            report.UpdatedAt = DateTime.UtcNow;
            report.PhotoAfterUrl = photoAfterUrl;

            // Если статус Completed - записываем время завершения
            if (status == ReportStatus.Completed && report.ClosedAt == null)
            {
                report.ClosedAt = DateTime.UtcNow;
                
                // Начисляем монеты пользователю за выполненное обращение
                await AwardCoinsForCompletedReport(report.AuthorId, cancellationToken);
            }

            // Назначаем оператора если ещё не назначен
            if (report.AssignedToId == null)
            {
                report.AssignedToId = operatorId;
            }

            await context.SaveChangesAsync(cancellationToken);

            // Логируем в audit log
            await CreateAuditLogAsync(reportId, operatorId, oldStatus, status, comment, cancellationToken);

            logger.LogInformation(
                "Report {ReportId} status changed from {OldStatus} to {NewStatus} by operator {OperatorId}",
                reportId, oldStatus, status, operatorId);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating report status for report {ReportId}", reportId);
            throw;
        }
    }

    public async Task<bool> AddPhotoAfterAsync(Guid reportId, string photoUrl, CancellationToken cancellationToken = default)
    {
        try
        {
            var report = await context.Reports.FindAsync([reportId], cancellationToken);
            
            if (report == null)
            {
                logger.LogWarning("Report {ReportId} not found", reportId);
                return false;
            }

            // TODO: добавить поле PhotoAfter в Report entity
            // report.PhotoAfter = photoUrl;
            report.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Photo after added to report {ReportId}", reportId);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding photo after for report {ReportId}", reportId);
            throw;
        }
    }

    public async Task<OperatorStatsDto> GetStatsAsync(string period = "today", CancellationToken cancellationToken = default)
    {
        try
        {
            var (fromDate, toDate) = GetDateRangeForPeriod(period);

            var query = context.Reports
                .Where(r => r.CreatedAt >= fromDate && r.CreatedAt <= toDate);

            var total = await query.CountAsync(cancellationToken);
            var newCount = await query.CountAsync(r => r.Status == ReportStatus.New, cancellationToken);
            var inProgressCount = await query.CountAsync(r => r.Status == ReportStatus.InProgress, cancellationToken);
            var completedCount = await query.CountAsync(r => r.Status == ReportStatus.Completed, cancellationToken);
            var rejectedCount = await query.CountAsync(r => r.Status == ReportStatus.Rejected, cancellationToken);

            return new OperatorStatsDto
            {
                Total = total,
                New = newCount,
                InProgress = inProgressCount,
                Done = completedCount,
                Cancelled = rejectedCount,
                Period = period,
                FromDate = fromDate,
                ToDate = toDate
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting stats for period {Period}", period);
            throw;
        }
    }

    public async Task<bool> AssignReportAsync(Guid reportId, Guid operatorId, CancellationToken cancellationToken = default)
    {
        try
        {
            var report = await context.Reports.FindAsync(new object[] { reportId }, cancellationToken);
            
            if (report == null)
            {
                logger.LogWarning("Report {ReportId} not found", reportId);
                return false;
            }

            report.AssignedToId = operatorId;
            report.UpdatedAt = DateTime.UtcNow;

            // Если статус New - переводим в InProgress
            if (report.Status == ReportStatus.New)
            {
                report.Status = ReportStatus.InProgress;
            }

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Report {ReportId} assigned to operator {OperatorId}", reportId, operatorId);

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error assigning report {ReportId} to operator {OperatorId}", reportId, operatorId);
            throw;
        }
    }

    private async Task CreateAuditLogAsync(
        Guid reportId,
        Guid operatorId,
        ReportStatus oldStatus,
        ReportStatus newStatus,
        string? comment,
        CancellationToken cancellationToken)
    {
        try
        {
            var operatorUser = await context.Users.FindAsync([operatorId], cancellationToken);
            
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                EntityType = "Report",
                EntityId = reportId,
                Action = AuditAction.Updated,
                UserId = operatorId,
                User = operatorUser,
                UserName = operatorUser != null ? $"{operatorUser.FirstName} {operatorUser.LastName}" : null,
                OldValues = $"{{\"Status\": \"{oldStatus}\"}}",
                NewValues = $"{{\"Status\": \"{newStatus}\", \"Comment\": \"{comment}\"}}",
                Description = comment,
                CreatedAt = DateTime.UtcNow,
                Timestamp = DateTime.UtcNow
            };

            context.AuditLogs.Add(auditLog);
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating audit log for report {ReportId}", reportId);
            // Не прерываем основной процесс из-за ошибки логирования
        }
    }

    private async Task AwardCoinsForCompletedReport(Guid userId, CancellationToken cancellationToken)
    {
        try
        {
            const int coinsReward = 10; // Награда за выполненное обращение

            var user = await context.Users.FindAsync([userId], cancellationToken);
            if (user != null)
            {
                user.CoinBalance += coinsReward;

                var coinTransaction = new CoinTransaction
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Amount = coinsReward,
                    Reason = CoinTransactionReason.ReportCompleted,
                    Type = CoinTransactionType.Credit,
                    BalanceAfter = user.CoinBalance,
                    Description = "Награда за выполненное обращение",
                    CreatedAt = DateTime.UtcNow
                };

                context.CoinTransactions.Add(coinTransaction);
                await context.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Awarded {Coins} coins to user {UserId} for completed report", coinsReward, userId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error awarding coins to user {UserId}", userId);
            // Не прерываем основной процесс
        }
    }

    private (DateTime fromDate, DateTime toDate) GetDateRangeForPeriod(string period)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;

        return period.ToLower() switch
        {
            "today" => (today, today.AddDays(1).AddTicks(-1)),
            "week" => (today.AddDays(-7), now),
            "month" => (today.AddDays(-30), now),
            "year" => (today.AddDays(-365), now),
            _ => (today, today.AddDays(1).AddTicks(-1))
        };
    }

    private static string GetCategoryName(ReportCategory category)
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

    private static string GetCategoryNameStatic(ReportCategory category)
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
}
