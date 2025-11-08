using TazaOrda.Domain.DTOs.Coins;

namespace TazaOrda.Domain.Interfaces;

/// <summary>
/// Интерфейс сервиса для работы с coins (внутренней валютой)
/// </summary>
public interface ICoinService
{
    /// <summary>
    /// Получить историю транзакций пользователя
    /// </summary>
    Task<List<CoinHistoryDto>> GetCoinHistoryAsync(Guid userId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить баланс пользователя
    /// </summary>
    Task<CoinBalanceDto> GetCoinBalanceAsync(Guid userId, CancellationToken cancellationToken = default);
}
