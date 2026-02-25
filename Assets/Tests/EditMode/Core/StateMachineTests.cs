using NUnit.Framework;
using ValenthiaChronicles.Core.Patterns;

namespace ValenthiaChronicles.Tests.EditMode.Core
{
    public class StateMachineTests
    {
        private class MockOwner { }

        private class MockState : IState
        {
            public bool Entered;
            public bool Executed;
            public bool Exited;

            public void Enter() => Entered = true;
            public void Execute() => Executed = true;
            public void Exit() => Exited = true;
        }

        [Test]
        public void ChangeState_ToNewState_CallsEnter()
        {
            var sm = new StateMachine<MockOwner>(new MockOwner());
            var state = new MockState();

            sm.ChangeState(state);

            Assert.IsTrue(state.Entered);
            Assert.AreEqual(state, sm.CurrentState);
        }

        [Test]
        public void ChangeState_ToNull_DoesNothing()
        {
            var sm = new StateMachine<MockOwner>(new MockOwner());
            var state = new MockState();
            sm.ChangeState(state);

            sm.ChangeState(null);

            Assert.AreEqual(state, sm.CurrentState);
        }

        [Test]
        public void ChangeState_FromExistingState_CallsExitThenEnter()
        {
            var sm = new StateMachine<MockOwner>(new MockOwner());
            var stateA = new MockState();
            var stateB = new MockState();

            sm.ChangeState(stateA);
            sm.ChangeState(stateB);

            Assert.IsTrue(stateA.Exited);
            Assert.IsTrue(stateB.Entered);
        }

        [Test]
        public void ChangeState_SetsPreviousState()
        {
            var sm = new StateMachine<MockOwner>(new MockOwner());
            var stateA = new MockState();
            var stateB = new MockState();

            sm.ChangeState(stateA);
            sm.ChangeState(stateB);

            Assert.AreEqual(stateA, sm.PreviousState);
            Assert.AreEqual(stateB, sm.CurrentState);
        }

        [Test]
        public void Update_CallsExecuteOnCurrentState()
        {
            var sm = new StateMachine<MockOwner>(new MockOwner());
            var state = new MockState();
            sm.ChangeState(state);

            sm.Update();

            Assert.IsTrue(state.Executed);
        }

        [Test]
        public void Update_WithNoState_DoesNotThrow()
        {
            var sm = new StateMachine<MockOwner>(new MockOwner());

            Assert.DoesNotThrow(() => sm.Update());
        }

        [Test]
        public void RevertToPreviousState_RestoresPreviousState()
        {
            var sm = new StateMachine<MockOwner>(new MockOwner());
            var stateA = new MockState();
            var stateB = new MockState();

            sm.ChangeState(stateA);
            sm.ChangeState(stateB);
            sm.RevertToPreviousState();

            Assert.AreEqual(stateA, sm.CurrentState);
        }

        [Test]
        public void RevertToPreviousState_WithNoPrevious_DoesNothing()
        {
            var sm = new StateMachine<MockOwner>(new MockOwner());
            var state = new MockState();
            sm.ChangeState(state);

            sm.RevertToPreviousState();

            Assert.AreEqual(state, sm.CurrentState);
        }
    }
}
