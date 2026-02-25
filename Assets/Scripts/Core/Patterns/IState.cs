namespace ValenthiaChronicles.Core.Patterns
{
    public interface IState
    {
        void Enter();
        void Execute();
        void Exit();
    }
}
