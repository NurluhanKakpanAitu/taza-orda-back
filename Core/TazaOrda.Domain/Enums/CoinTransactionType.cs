namespace TazaOrda.Domain.Enums;

/// <summary>
/// Типы транзакций с внутренней валютой (coins)
/// </summary>
public enum CoinTransactionType
{
    /// <summary>
    /// Начисление - пополнение баланса
    /// </summary>
    Credit = 0,

    /// <summary>
    /// Списание - расход баланса
    /// </summary>
    Debit = 1
}