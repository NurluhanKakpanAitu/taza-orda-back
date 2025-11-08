namespace TazaOrda.Domain.DTOs.Coins;

/// <summary>
/// DTO истории транзакций с coins
/// </summary>
public record CoinHistoryDto
{
    public Guid Id { get; init; }
    public DateTime Date { get; init; }
    public string Reason { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Type { get; init; } = string.Empty; // Credit или Debit
    public decimal BalanceAfter { get; init; }
    public string? Description { get; init; }
}

/// <summary>
/// DTO баланса пользователя
/// </summary>
public record CoinBalanceDto
{
    public decimal CurrentBalance { get; init; }
    public decimal TotalEarned { get; init; }
    public decimal TotalSpent { get; init; }
}
