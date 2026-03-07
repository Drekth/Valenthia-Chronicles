# Story 1.2: Core Patterns Implementation

Status: ready-for-dev

## Story

As a **developer**,
I want **reusable core patterns (StateMachine, ObjectPool, ICommand, GameConstants) implemented in Scripts/Core/**,
so that **all future systems use consistent, tested patterns instead of ad-hoc implementations**.

## Acceptance Criteria

1. **StateMachine<T> implemented**
   - `IState` interface with `Enter()`, `Execute()`, `Exit()`
   - `StateMachine<T>` generic class with `CurrentState`, `PreviousState`
   - Supports hierarchical sub-state machines
   - State transitions go THROUGH the machine, never directly between states
   - Logs transitions via `GameLogger.Dbg()` with `[StateMachine]` tag

2. **ObjectPool<T> implemented**
   - `IObjectPool<T>` interface with `Get()`, `Release()`, `PreWarm()`
   - Generic `ObjectPool<T>` implementation for `Component` types
   - Auto-expand with warning log when pool exhausted
   - Release = `SetActive(false)` + return to pool, NEVER `Destroy()`

3. **ICommand pattern implemented**
   - `ICommand` interface with `Execute()` and optional `Undo()`
   - `CommandQueue` class for sequential command execution
   - Commands are data objects — serializable for replay/save

4. **GameConstants defined**
   - `GameConstants` static class in `Scripts/Core/Data/`
   - Constants use `UPPER_SNAKE_CASE`
   - Contains initial constants: `MAX_LEVEL`, `INVENTORY_SLOTS`, `DEFAULT_MOVE_SPEED`, etc.

5. **Edit Mode tests validate all patterns**
   - StateMachine transition tests
   - ObjectPool get/release/prewarm tests
   - CommandQueue execute/undo tests

## USER Manual Steps (Unity Editor)

**⚠️ Ces étapes DOIVENT être faites manuellement par l'utilisateur dans Unity :**

### Step 1: Verify Compilation
1. Retourner dans Unity après l'implémentation LLM
2. Attendre la recompilation automatique
3. Vérifier qu'il n'y a **aucune erreur** dans la Console

### Step 2: Run Tests
1. Ouvrir `Window` → `General` → `Test Runner`
2. Onglet `EditMode` → Cliquer `Run All`
3. Vérifier que tous les tests passent ✅ (incluant Story 1.1 + 1.2)

---

## LLM Automated Tasks

**✅ Ces tâches seront automatiquement effectuées par le LLM dev-story :**

- [ ] **Task 1: Create IState and StateMachine<T>** (AC: #1)
  - [ ] Create `IState.cs` in `Scripts/Core/Patterns/`
  - [ ] Create `StateMachine.cs` in `Scripts/Core/Patterns/`
  - [ ] Implement `ChangeState()`, `Update()`, `CurrentState`, `PreviousState`
  - [ ] Support hierarchical sub-state machines
  - [ ] Log transitions via `GameLogger.Dbg("[StateMachine] ...")`

- [ ] **Task 2: Create IObjectPool<T> and ObjectPool<T>** (AC: #2)
  - [ ] Create `IObjectPool.cs` in `Scripts/Core/Patterns/`
  - [ ] Create `ObjectPool.cs` in `Scripts/Core/Patterns/`
  - [ ] Implement `Get()`, `Release()`, `PreWarm()`
  - [ ] Auto-expand with `GameLogger.Warn()` when exhausted
  - [ ] Release = `SetActive(false)`, Get = `SetActive(true)`

- [ ] **Task 3: Create ICommand and CommandQueue** (AC: #3)
  - [ ] Create `ICommand.cs` in `Scripts/Core/Patterns/`
  - [ ] Create `CommandQueue.cs` in `Scripts/Core/Patterns/`
  - [ ] `ICommand` with `Execute()` and `Undo()`
  - [ ] `CommandQueue` for sequential execution with history

- [ ] **Task 4: Create GameConstants** (AC: #4)
  - [ ] Create `GameConstants.cs` in `Scripts/Core/Data/`
  - [ ] Define initial constants with `UPPER_SNAKE_CASE`

- [ ] **Task 5: Write Edit Mode tests** (AC: #5)
  - [ ] Create `StateMachineTests.cs` in `Assets/Tests/EditMode/Core/`
  - [ ] Create `ObjectPoolTests.cs` in `Assets/Tests/EditMode/Core/`
  - [ ] Create `CommandQueueTests.cs` in `Assets/Tests/EditMode/Core/`

## Dev Notes

### Architecture Compliance

**Design Patterns from Architecture:**
- **HSM (Hierarchical State Machine):** States are classes, not enums. Transitions through machine only.
- **Object Pool:** Frequent entities (enemies, projectiles, VFX, damage numbers) ALWAYS pooled. Pre-warm on zone load.
- **Command Pattern:** Companion orders, ability execution, input actions.
- **Data-Driven:** `GameConstants` for compile-time values, ScriptableObjects for balance data.

### Dependencies

- **Story 1.1 (done):** `GameLogger` already exists in `Scripts/Core/Utils/`
- **No Unity packages required** — pure C# patterns (except ObjectPool needs `UnityEngine.Component`)

### File Structure

```
Assets/Scripts/Core/
├── Patterns/
│   ├── IState.cs
│   ├── StateMachine.cs
│   ├── IObjectPool.cs
│   ├── ObjectPool.cs
│   ├── ICommand.cs
│   └── CommandQueue.cs
├── Data/
│   └── GameConstants.cs
└── (existing from 1.1)
    ├── Interfaces/IGameManager.cs
    ├── Services/GameManager.cs
    └── Utils/GameLogger.cs
```

### Code Templates

**IState.cs:**
```csharp
namespace ValenthiaChronicles.Core.Patterns
{
    public interface IState
    {
        void Enter();
        void Execute();
        void Exit();
    }
}
```

**StateMachine.cs:**
```csharp
namespace ValenthiaChronicles.Core.Patterns
{
    public class StateMachine<T> where T : class
    {
        public IState CurrentState { get; private set; }
        public IState PreviousState { get; private set; }
        private readonly T _owner;

        public StateMachine(T owner) { _owner = owner; }

        public void ChangeState(IState newState)
        {
            if (newState == null) return;
            PreviousState = CurrentState;
            CurrentState?.Exit();
            CurrentState = newState;
            GameLogger.Dbg($"[StateMachine] {typeof(T).Name}: {PreviousState?.GetType().Name ?? "None"} → {newState.GetType().Name}");
            CurrentState.Enter();
        }

        public void Update() => CurrentState?.Execute();

        public void RevertToPreviousState()
        {
            if (PreviousState != null) ChangeState(PreviousState);
        }
    }
}
```

**IObjectPool.cs:**
```csharp
using UnityEngine;

namespace ValenthiaChronicles.Core.Patterns
{
    public interface IObjectPool<T> where T : Component
    {
        T Get();
        void Release(T instance);
        void PreWarm(int count);
        int CountActive { get; }
        int CountInactive { get; }
    }
}
```

**ObjectPool.cs:**
```csharp
using System.Collections.Generic;
using UnityEngine;

namespace ValenthiaChronicles.Core.Patterns
{
    public class ObjectPool<T> : IObjectPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Queue<T> _pool = new();
        private readonly List<T> _active = new();

        public int CountActive => _active.Count;
        public int CountInactive => _pool.Count;

        public ObjectPool(T prefab, Transform parent = null, int initialSize = 0)
        {
            _prefab = prefab;
            _parent = parent;
            if (initialSize > 0) PreWarm(initialSize);
        }

        public void PreWarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var instance = Object.Instantiate(_prefab, _parent);
                instance.gameObject.SetActive(false);
                _pool.Enqueue(instance);
            }
        }

        public T Get()
        {
            T instance;
            if (_pool.Count > 0)
            {
                instance = _pool.Dequeue();
            }
            else
            {
                GameLogger.Warn($"[ObjectPool] Pool<{typeof(T).Name}> exhausted — auto-expanding");
                instance = Object.Instantiate(_prefab, _parent);
            }
            instance.gameObject.SetActive(true);
            _active.Add(instance);
            return instance;
        }

        public void Release(T instance)
        {
            instance.gameObject.SetActive(false);
            _active.Remove(instance);
            _pool.Enqueue(instance);
        }
    }
}
```

**ICommand.cs:**
```csharp
namespace ValenthiaChronicles.Core.Patterns
{
    public interface ICommand
    {
        void Execute();
        void Undo();
    }
}
```

**CommandQueue.cs:**
```csharp
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
```

**GameConstants.cs:**
```csharp
namespace ValenthiaChronicles.Core
{
    public static class GameConstants
    {
        // Progression
        public const int MAX_LEVEL = 60;
        public const float BASE_XP_PER_LEVEL = 100f;
        public const float XP_SCALING_FACTOR = 1.15f;

        // Inventory
        public const int INVENTORY_SLOTS = 24;
        public const int EQUIPMENT_SLOTS = 10;

        // Movement
        public const float DEFAULT_MOVE_SPEED = 5f;
        public const float DEFAULT_RUN_MULTIPLIER = 1.5f;

        // Combat
        public const float GLOBAL_COOLDOWN = 1.5f;
        public const float DEFAULT_ATTACK_RANGE = 2f;
        public const int MAX_ACTIVE_STATUS_EFFECTS = 20;

        // Pooling
        public const int DEFAULT_POOL_SIZE = 10;
        public const int PROJECTILE_POOL_SIZE = 20;
        public const int VFX_POOL_SIZE = 15;
        public const int DAMAGE_NUMBER_POOL_SIZE = 30;

        // Save
        public const int MAX_SAVE_SLOTS = 5;
        public const int AUTOSAVE_INTERVAL_SECONDS = 300;

        // Camera
        public const float DEFAULT_CAMERA_HEIGHT = 15f;
        public const float DEFAULT_CAMERA_ANGLE = 45f;
        public const float MIN_ZOOM = 8f;
        public const float MAX_ZOOM = 25f;
    }
}
```

### Testing Approach

**StateMachineTests.cs:**
```csharp
using NUnit.Framework;
using ValenthiaChronicles.Core.Patterns;

namespace ValenthiaChronicles.Tests.EditMode.Core
{
    public class StateMachineTests
    {
        private class MockOwner { }
        private class MockState : IState
        {
            public bool Entered, Executed, Exited;
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
        public void Update_CallsExecuteOnCurrentState()
        {
            var sm = new StateMachine<MockOwner>(new MockOwner());
            var state = new MockState();
            sm.ChangeState(state);
            sm.Update();
            Assert.IsTrue(state.Executed);
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
    }
}
```

### Critical Rules (from Architecture)

- States are **classes**, not enums — allows logic encapsulation
- State transitions go **through the machine**, never directly between states
- NEVER `Instantiate`/`Destroy` frequently spawned objects — always pool
- Pool pre-warm on zone load to avoid frame spikes
- Commands are data objects — serializable for replay/save
- Constants use `UPPER_SNAKE_CASE` in `GameConstants` — NO magic numbers

### References

- [Source: _bmad-output/game-architecture.md#Hierarchical-State-Machine] — IState + StateMachine<T>
- [Source: _bmad-output/game-architecture.md#Object-Pool] — IObjectPool<T>
- [Source: _bmad-output/game-architecture.md#Command-Pattern] — ICommand
- [Source: _bmad-output/game-architecture.md#Configuration-Management] — GameConstants
- [Source: _bmad-output/game-architecture.md#Consistency-Rules] — Naming conventions

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

