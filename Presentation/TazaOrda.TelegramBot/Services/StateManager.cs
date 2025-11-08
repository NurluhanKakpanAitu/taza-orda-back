using System.Collections.Concurrent;
using TazaOrda.TelegramBot.Models;

namespace TazaOrda.TelegramBot.Services;

/// <summary>
/// Менеджер состояний пользователей
/// </summary>
public class StateManager
{
    private readonly ConcurrentDictionary<long, UserState> _userStates = new();

    public UserState GetOrCreateState(long telegramId)
    {
        return _userStates.GetOrAdd(telegramId, id => new UserState { TelegramId = id });
    }

    public void UpdateState(long telegramId, Action<UserState> updateAction)
    {
        var state = GetOrCreateState(telegramId);
        updateAction(state);
        state.LastActivity = DateTime.UtcNow;
    }

    public void ClearState(long telegramId)
    {
        _userStates.TryRemove(telegramId, out _);
    }

    public void ResetConversation(long telegramId)
    {
        UpdateState(telegramId, state =>
        {
            state.State = ConversationState.None;
            state.Data.Clear();
        });
    }

    public T? GetData<T>(long telegramId, string key) where T : class
    {
        var state = GetOrCreateState(telegramId);
        return state.Data.TryGetValue(key, out var value) ? value as T : null;
    }

    public void SetData<T>(long telegramId, string key, T value) where T : class
    {
        UpdateState(telegramId, state =>
        {
            state.Data[key] = value;
        });
    }

    // Очистка неактивных состояний (старше 24 часов)
    public void CleanupInactiveStates()
    {
        var threshold = DateTime.UtcNow.AddHours(-24);
        var inactiveKeys = _userStates
            .Where(kv => kv.Value.LastActivity < threshold)
            .Select(kv => kv.Key)
            .ToList();

        foreach (var key in inactiveKeys)
        {
            _userStates.TryRemove(key, out _);
        }
    }
}