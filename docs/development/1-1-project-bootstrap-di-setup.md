# Story 1.1: Project Bootstrap & DI Setup

Status: ready-for-dev

## Story

As a **developer**,
I want **Init(args) DI configured with a GameManager global service and a bootstrap scene**,
so that **all future systems have a clean dependency injection foundation and the game initializes properly**.

## Acceptance Criteria

1. **Init(args) package installed and configured**
   - Package added via Asset Store or OpenUPM
   - Null Argument Guard enabled (Inspector + Runtime)
   - Verified working in Editor

2. **GameManager service created**
   - `[Service]` attribute applied
   - Implements basic game state (Loading, MainMenu, Playing, Paused)
   - Uses `StateMachine<GameState>` pattern from Core patterns
   - Logs initialization via `GameLogger`

3. **Bootstrap scene functional**
   - Scene named `SC_Bootstrap.unity` in `Assets/Scenes/`
   - Loads before any gameplay scene
   - GameManager initializes and transitions to MainMenu state
   - Scene set as Build Index 0

4. **Service interfaces scaffolded**
   - `IGameManager` interface in `Scripts/Core/Interfaces/`
   - GameManager registered as `[Service(typeof(IGameManager))]`

5. **Basic test validates DI**
   - Edit Mode test confirms GameManager service resolution
   - Test in `Assets/Tests/EditMode/Core/GameManagerTests.cs`

## USER Manual Steps (Unity Editor)

**⚠️ Ces étapes DOIVENT être faites manuellement par l'utilisateur dans Unity :**

### Step 1: Install Init(args) Package
**Méthode A - Asset Store (Recommandé) :**
1. Ouvrir Unity Hub → Lancer le projet
2. Dans Unity Editor : `Window` → `Asset Store`
3. Rechercher "Init(args)" par Sisus
4. Cliquer `Download` puis `Import`
5. Accepter tous les fichiers dans la fenêtre d'import

**Méthode B - OpenUPM (Terminal) :**
```bash
cd /Users/mburtin/Documents/GitHub/Valenthia-Chronicles
openupm add com.sisus.init-args
```

**Vérification :**
- Ouvrir `Window` → `Package Manager`
- Vérifier que "Init(args)" apparaît dans la liste

---

### Step 2: Create Bootstrap Scene
1. `File` → `New Scene` → `Empty Scene`
2. `File` → `Save As...` → Nommer `SC_Bootstrap.unity`
3. Sauvegarder dans `Assets/Scenes/` (créer le dossier si nécessaire)
4. `File` → `Build Settings...`
5. Glisser `SC_Bootstrap` dans la liste des scènes
6. Le placer en **position 0** (première scène à charger)

---

### Step 3: Verify Installation (After LLM Implementation)
1. Appuyer sur **Play** dans `SC_Bootstrap`
2. Vérifier dans la **Console** :
   - `[GameManager] Initialized`
   - `[GameManager] State: Loading → MainMenu`
3. Ouvrir `Window` → `General` → `Test Runner`
4. Onglet `EditMode` → Cliquer `Run All`
5. Vérifier que tous les tests passent ✅

---

## LLM Automated Tasks

**✅ Ces tâches seront automatiquement effectuées par le LLM dev-story :**

- [ ] **Task 1: Create folder structure** (AC: #2, #4)
  - [ ] Create `Assets/Scripts/Core/Interfaces/`
  - [ ] Create `Assets/Scripts/Core/Services/`
  - [ ] Create `Assets/Scripts/Core/Utils/`
  - [ ] Create `Assets/Tests/EditMode/Core/`

- [ ] **Task 2: Implement IGameManager interface** (AC: #4)
  - [ ] Create `IGameManager.cs` in `Scripts/Core/Interfaces/`
  - [ ] Define `GameState` enum (Loading, MainMenu, Playing, Paused)
  - [ ] Define `CurrentState` property
  - [ ] Define `ChangeState(GameState)` method
  - [ ] Define state change event

- [ ] **Task 3: Implement GameLogger utility** (AC: #2)
  - [ ] Create `GameLogger.cs` in `Scripts/Core/Utils/`
  - [ ] Implement `Info`, `Warn`, `Error` methods with `[Tag]` format
  - [ ] Implement `Debug` method with `[Conditional("UNITY_EDITOR")]`

- [ ] **Task 4: Implement GameManager service** (AC: #2, #4)
  - [ ] Create `GameManager.cs` in `Scripts/Core/Services/`
  - [ ] Apply `[Service(typeof(IGameManager))]` attribute
  - [ ] Implement `IGameManager` interface
  - [ ] Use simple state machine for game states
  - [ ] Log state transitions via `GameLogger`

- [ ] **Task 5: Write Edit Mode test** (AC: #5)
  - [ ] Create `GameManagerTests.cs` in `Assets/Tests/EditMode/Core/`
  - [ ] Test GameManager state transitions
  - [ ] Test state change events
  - [ ] Verify test passes in Test Runner

## Dev Notes

### Architecture Compliance

**Core Paradigm:** MonoBehaviour + Init(args) DI
- All services use `[Service]` attribute — NO singletons
- Services expose interfaces (`IGameManager`) for testability
- Global services initialize before scene load

**Design Patterns Required:**
- **State Machine** for `GameState` transitions
- **Observer/Event** for state change notifications (via interface events)

### Technical Requirements

| Requirement | Value | Source |
|-------------|-------|--------|
| Unity Version | 6 (6000.3.9f1) | Architecture |
| Init(args) | Latest | Architecture |
| C# Version | Latest Unity 6 supported | Project Context |
| Namespace | `Sisus.Init` required | Init(args) Guide |

### File Structure

```
Assets/
├── Scripts/
│   └── Core/
│       ├── Interfaces/
│       │   └── IGameManager.cs
│       ├── Services/
│       │   └── GameManager.cs
│       ├── Patterns/
│       │   └── (Story 1.2)
│       └── Utils/
│           └── GameLogger.cs
├── Scenes/
│   └── SC_Bootstrap.unity
└── Tests/
    └── EditMode/
        └── Core/
            └── GameManagerTests.cs
```

### Code Templates

**IGameManager.cs:**
```csharp
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
        event Action<GameState, GameState> OnStateChanged; // (oldState, newState)
        void ChangeState(GameState newState);
    }
}
```

**GameManager.cs:**
```csharp
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
```

**GameLogger.cs:**
```csharp
using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace ValenthiaChronicles.Core
{
    public static class GameLogger
    {
        public static void Info(string message) => Debug.Log(message);
        public static void Warn(string message) => Debug.LogWarning(message);
        public static void Error(string message, Exception ex = null) =>
            Debug.LogError(ex != null ? $"{message}: {ex.Message}" : message);

        [Conditional("UNITY_EDITOR")]
        public static void Debug(string message) => UnityEngine.Debug.Log($"[DBG] {message}");
    }
}
```

### Critical Rules (from Project Context)

- **NEVER** use `Singleton.Instance` — use `[Service]` instead
- **NEVER** use `FindObjectOfType` — use Init(args) injection
- Override `OnAwake()` NOT `Awake()` for MonoBehaviour<T>
- Override `OnReset()` NOT `Reset()` for MonoBehaviour<T>
- `using Sisus.Init;` namespace required in all Init(args) files
- Services expose **interfaces** for testability

### Testing Approach

**Edit Mode Test Pattern:**
```csharp
using NUnit.Framework;
using ValenthiaChronicles.Core;

namespace ValenthiaChronicles.Tests.EditMode.Core
{
    public class GameManagerTests
    {
        [Test]
        public void ChangeState_FromLoadingToMainMenu_UpdatesCurrentState()
        {
            // Arrange
            var manager = new GameManager();
            
            // Act
            manager.ChangeState(GameState.Playing);
            
            // Assert
            Assert.AreEqual(GameState.Playing, manager.CurrentState);
        }

        [Test]
        public void ChangeState_WhenStateChanges_FiresEvent()
        {
            // Arrange
            var manager = new GameManager();
            GameState? receivedOld = null;
            GameState? receivedNew = null;
            manager.OnStateChanged += (old, newState) => { receivedOld = old; receivedNew = newState; };
            
            // Act
            manager.ChangeState(GameState.Playing);
            
            // Assert
            Assert.AreEqual(GameState.MainMenu, receivedOld);
            Assert.AreEqual(GameState.Playing, receivedNew);
        }
    }
}
```

### Project Structure Notes

- `Scripts/Core/` is the foundation — depends on NOTHING else
- Assembly Definition `ValenthiaChronicles.Core.asmdef` will be created in Story 1.4
- For now, scripts can be in default assembly

### References

- [Source: docs/init-args-guide.md#1-services-globaux] — `[Service]` pattern
- [Source: _bmad-output/game-architecture.md#Core-Paradigm] — MonoBehaviour + Init(args) DI
- [Source: _bmad-output/game-architecture.md#Logging] — GameLogger format `[Tag] message`
- [Source: _bmad-output/project-context.md#Critical-Implementation-Rules] — Anti-patterns to avoid

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

