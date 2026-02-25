using NUnit.Framework;
using ValenthiaChronicles.Core.Patterns;

namespace ValenthiaChronicles.Tests.EditMode.Core
{
    public class CommandQueueTests
    {
        private class MockCommand : ICommand
        {
            public bool Executed;
            public bool Undone;

            public void Execute() => Executed = true;
            public void Undo() => Undone = true;
        }

        [Test]
        public void Execute_CallsCommandExecute()
        {
            var queue = new CommandQueue();
            var cmd = new MockCommand();

            queue.Execute(cmd);

            Assert.IsTrue(cmd.Executed);
        }

        [Test]
        public void Execute_AddsToHistory()
        {
            var queue = new CommandQueue();

            queue.Execute(new MockCommand());
            queue.Execute(new MockCommand());

            Assert.AreEqual(2, queue.HistoryCount);
        }

        [Test]
        public void Undo_CallsCommandUndo()
        {
            var queue = new CommandQueue();
            var cmd = new MockCommand();
            queue.Execute(cmd);

            var result = queue.Undo();

            Assert.IsTrue(result);
            Assert.IsTrue(cmd.Undone);
        }

        [Test]
        public void Undo_RemovesFromHistory()
        {
            var queue = new CommandQueue();
            queue.Execute(new MockCommand());
            queue.Execute(new MockCommand());

            queue.Undo();

            Assert.AreEqual(1, queue.HistoryCount);
        }

        [Test]
        public void Undo_WhenEmpty_ReturnsFalse()
        {
            var queue = new CommandQueue();

            var result = queue.Undo();

            Assert.IsFalse(result);
        }

        [Test]
        public void Undo_MultipleCommands_UndoesInReverseOrder()
        {
            var queue = new CommandQueue();
            var cmd1 = new MockCommand();
            var cmd2 = new MockCommand();
            queue.Execute(cmd1);
            queue.Execute(cmd2);

            queue.Undo();

            Assert.IsTrue(cmd2.Undone);
            Assert.IsFalse(cmd1.Undone);
        }

        [Test]
        public void Clear_EmptiesHistory()
        {
            var queue = new CommandQueue();
            queue.Execute(new MockCommand());
            queue.Execute(new MockCommand());

            queue.Clear();

            Assert.AreEqual(0, queue.HistoryCount);
        }
    }
}
