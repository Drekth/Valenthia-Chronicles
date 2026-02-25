using System;
using Sisus.Init;
using ValenthiaChronicles.Core;

[Service(typeof(IGameManager))]
public class GameManager : IGameManager
{
    public GameState CurrentState { get; private set; } = GameState.Loading;
    public event Action<GameState, GameState> OnStateChanged;

    public GameManager()
    {
        GameLogger.Info("[GameManager] Initialized");
        ChangeState(GameState.MainMenu);
    }

    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return;

        var oldState = CurrentState;
        CurrentState = newState;
        GameLogger.Info($"[GameManager] State: {oldState} → {newState}");
        OnStateChanged?.Invoke(oldState, newState);
    }
}
