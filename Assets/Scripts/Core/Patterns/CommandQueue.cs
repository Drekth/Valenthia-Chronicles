using System.Collections.Generic;

namespace ValenthiaChronicles.Core.Patterns
{
    public class CommandQueue
    {
        private readonly Stack<ICommand> _history = new();

        public int HistoryCount => _history.Count;

        public void Execute(ICommand command)
        {
            command.Execute();
            _history.Push(command);
        }

        public bool Undo()
        {
            if (_history.Count == 0) return false;
            var command = _history.Pop();
            command.Undo();
            return true;
        }

        public void Clear() => _history.Clear();
    }
}
