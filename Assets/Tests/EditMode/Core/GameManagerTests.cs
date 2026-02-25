using NUnit.Framework;
using ValenthiaChronicles.Core;

namespace ValenthiaChronicles.Tests.EditMode.Core
{
    public class GameManagerTests
    {
        [Test]
        public void Constructor_WhenCreated_TransitionsToMainMenu()
        {
            var manager = new GameManager();

            Assert.AreEqual(GameState.MainMenu, manager.CurrentState);
        }

        [Test]
        public void ChangeState_ToPlaying_UpdatesCurrentState()
        {
            var manager = new GameManager();

            manager.ChangeState(GameState.Playing);

            Assert.AreEqual(GameState.Playing, manager.CurrentState);
        }

        [Test]
        public void ChangeState_ToSameState_DoesNotFireEvent()
        {
            var manager = new GameManager();
            var eventFired = false;
            manager.OnStateChanged += (_, _) => eventFired = true;

            manager.ChangeState(GameState.MainMenu); // Already in MainMenu

            Assert.IsFalse(eventFired);
        }

        [Test]
        public void ChangeState_ToDifferentState_FiresEventWithCorrectValues()
        {
            var manager = new GameManager();
            GameState? receivedOld = null;
            GameState? receivedNew = null;
            manager.OnStateChanged += (oldState, newState) =>
            {
                receivedOld = oldState;
                receivedNew = newState;
            };

            manager.ChangeState(GameState.Playing);

            Assert.AreEqual(GameState.MainMenu, receivedOld);
            Assert.AreEqual(GameState.Playing, receivedNew);
        }

        [Test]
        public void ChangeState_MultipleTimes_TracksAllTransitions()
        {
            var manager = new GameManager();
            var transitionCount = 0;
            manager.OnStateChanged += (_, _) => transitionCount++;

            manager.ChangeState(GameState.Playing);
            manager.ChangeState(GameState.Paused);
            manager.ChangeState(GameState.Playing);

            Assert.AreEqual(3, transitionCount);
            Assert.AreEqual(GameState.Playing, manager.CurrentState);
        }
    }
}
