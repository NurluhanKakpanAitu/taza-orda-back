using Microsoft.EntityFrameworkCore;
using TazaOrda.Domain.DTOs.Coins;
using TazaOrda.Domain.Enums;
using TazaOrda.Domain.Interfaces;
using TazaOrda.Infrastructure.Persistence;

namespace TazaOrda.Infrastructure.Services.Coins;

/// <summary>
/// Реализация сервиса для работы с coins (внутренней валютой)
/// </summary>
public class CoinService : ICoinService
{
    private readonly TazaOrdaDbContext _context;

    public CoinService(TazaOrdaDbContext context)
    {
        _context = context;
    }

    public async Task<List<CoinHistoryDto>> GetCoinHistoryAsync(Guid userId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var transactions = await _context.CoinTransactions
            .Where(ct => ct.UserId == userId)
            .OrderByDescending(ct => ct.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ct => new CoinHistoryDto
            {
                Id = ct.Id,
                Date = ct.CreatedAt,
                Reason = ct.Reason.ToString(),
                Amount = ct.Type == CoinTransactionType.Credit ? ct.Amount : -ct.Amount,
                Type = ct.Type.ToString(),
                BalanceAfter = ct.BalanceAfter,
                Description = ct.Description
            })
            .ToListAsync(cancellationToken);

        return transactions;
    }

    public async Task<CoinBalanceDto> GetCoinBalanceAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
        
        if (user == null)
        {
            throw new InvalidOperationException("Пользователь не найден");
        }

        var totalEarned = await _context.CoinTransactions
            .Where(ct => ct.UserId == userId && ct.Type == CoinTransactionType.Credit)
            .SumAsync(ct => ct.Amount, cancellationToken);

        var totalSpent = await _context.CoinTransactions
            .Where(ct => ct.UserId == userId && ct.Type == CoinTransactionType.Debit)
            .SumAsync(ct => ct.Amount, cancellationToken);

        return new CoinBalanceDto
        {
            CurrentBalance = user.CoinBalance,
            TotalEarned = totalEarned,
            TotalSpent = totalSpent
        };
    }
}
