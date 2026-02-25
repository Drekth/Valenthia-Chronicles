using System;

namespace ValenthiaChronicles.Core
{
    public enum GameState
    {
        Loading,
        MainMenu,
        Playing,
        Paused
    }

    public interface IGameManager
    {
        GameState CurrentState { get; }
        event Action<GameState, GameState> OnStateChanged;
        void ChangeState(GameState newState);
    }
}
