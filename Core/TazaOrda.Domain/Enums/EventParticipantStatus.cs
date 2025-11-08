namespace TazaOrda.Domain.Enums;

/// <summary>
/// Статусы участия в акции/мероприятии
/// </summary>
public enum EventParticipantStatus
{
    /// <summary>
    /// Присоединился - пользователь зарегистрировался на событие
    /// </summary>
    Joined = 0,

    /// <summary>
    /// Зарегистрирован - чек-ин выполнен (по QR/гео)
    /// </summary>
    CheckedIn = 1,

    /// <summary>
    /// Завершено - участие подтверждено, монеты начислены
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Отменено - пользователь отменил своё участие
    /// </summary>
    Cancelled = 3
}