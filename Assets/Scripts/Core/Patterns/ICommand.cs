namespace ValenthiaChronicles.Core.Patterns
{
    public interface ICommand
    {
        void Execute();
        void Undo();
    }
}
