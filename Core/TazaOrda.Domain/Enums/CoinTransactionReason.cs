namespace TazaOrda.Domain.Enums;

/// <summary>
/// Причины транзакций с внутренней валютой
/// </summary>
public enum CoinTransactionReason
{
    /// <summary>
    /// Отправка обращения/жалобы
    /// </summary>
    ReportSubmitted = 0,

    /// <summary>
    /// Подтверждение выполнения обращения
    /// </summary>
    ReportCompleted = 1,

    /// <summary>
    /// Участие в акции/субботнике
    /// </summary>
    EventParticipation = 2,

    /// <summary>
    /// Награда за активность
    /// </summary>
    ActivityReward = 3,

    /// <summary>
    /// Достижение уровня/рейтинга
    /// </summary>
    LevelAchievement = 4,

    /// <summary>
    /// Регистрация в системе (приветственный бонус)
    /// </summary>
    RegistrationBonus = 5,

    /// <summary>
    /// Обмен на награду/подарок
    /// </summary>
    RewardRedemption = 6,

    /// <summary>
    /// Административное начисление
    /// </summary>
    AdminAdjustment = 7,

    /// <summary>
    /// Бонус за рекомендацию (реферальная программа)
    /// </summary>
    ReferralBonus = 8,

    /// <summary>
    /// Отмена/корректировка транзакции
    /// </summary>
    Reversal = 9,

    /// <summary>
    /// Другое
    /// </summary>
    Other = 99
}