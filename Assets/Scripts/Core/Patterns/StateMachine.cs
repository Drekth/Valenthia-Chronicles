namespace ValenthiaChronicles.Core.Patterns
{
    public class StateMachine<T> where T : class
    {
        public IState CurrentState { get; private set; }
        public IState PreviousState { get; private set; }
        private readonly T _owner;

        public StateMachine(T owner)
        {
            _owner = owner;
        }

        public T Owner => _owner;

        public void ChangeState(IState newState)
        {
            if (newState == null) return;

            PreviousState = CurrentState;
            CurrentState?.Exit();
            CurrentState = newState;
            GameLogger.Dbg($"[StateMachine] {typeof(T).Name}: {PreviousState?.GetType().Name ?? "None"} → {newState.GetType().Name}");
            CurrentState.Enter();
        }

        public void Update()
        {
            CurrentState?.Execute();
        }

        public void RevertToPreviousState()
        {
            if (PreviousState != null)
                ChangeState(PreviousState);
        }
    }
}
