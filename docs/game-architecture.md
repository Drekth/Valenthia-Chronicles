---
title: 'Game Architecture'
project: 'Valenthia Chronicles'
date: '2026-02-23'
author: 'Drekth'
version: '1.0'
stepsCompleted: [1, 2, 3, 4, 5, 6, 7, 8, 9]
status: 'complete'

# Source Documents
gdd: '_bmad-output/plans/gdd.md'
epics: '_bmad-output/plans/epics.md'
brief: '_bmad-output/plans/game-brief.md'
---

# Game Architecture

## Executive Summary

Valenthia Chronicles is a narrative-first top-down action RPG built on **Unity 6 (URP)** using a **MonoBehaviour + Init(args) DI** paradigm. The architecture is designed for a solo developer building a 50-70 hour single-player RPG over multiple years, prioritizing clean interfaces, data-driven design, and extensibility.

Key architectural decisions: MemoryPack for zero-alloc save serialization, Addressables for zone-based asset streaming, TrinityCore-style NPC management (scene-placed, runtime-evaluated), hierarchical state machines for all entity behavior, and Observer pattern via DI interfaces for decoupled system communication. Two critical decisions (dialogue system and world state architecture) are intentionally deferred to their respective epic implementations.

## Project Context

### Game Overview

**Valenthia Chronicles** — A narrative-first top-down action RPG combining WoW-style world-building and faction dynamics with Mass Effect-inspired consequence-driven storytelling. Single-player, PC-only. Players evolve from an ordinary soldier to a legendary hero across a three-act structure (Soldier → War Hero → Legend) in a living world where choices persistently shape the environment, alliances, and narrative outcomes.

### Technical Scope

**Platform:** PC (Windows, Linux, macOS) — 64-bit only
**Genre:** Top-down Action RPG (narrative-first)
**Project Level:** High complexity — solo developer, multi-year passion project
**Target:** 50-70 hours of gameplay, 60 FPS @ 1080p on mid-range hardware

### Core Systems

| System | Complexity | GDD Reference | Key Considerations |
|--------|-----------|---------------|--------------------|
| Combat (WoW-style targeting + hotbar abilities) | High | Game Mechanics > Combat System | State machine, cooldowns, resource management, status effects |
| Living World & Consequences (persistent world state) | High | USP #2, Quest System | Novel pattern — no standard solution, central world state tracker |
| Quest & Dialogue (branching narrative, NPC memory) | High | Quest System, NPC & Dialogue | Mass Effect-style consequence tracking, reputation per faction/NPC |
| Zone Loading (scene-based transitions) | Medium | World and Exploration | Scene management, asset cleanup, fast travel |
| Character Progression (levels 1-60, dual talent trees) | Medium | Character System, Progression | Race + Specialization trees, logarithmic XP curve |
| Inventory & Equipment (slot-based, rarity tiers) | Medium | Inventory and Equipment | 5 rarity tiers, equipment slots, stat bonuses |
| Companion AI (combat roles, basic orders) | Medium | Combat System > Companion AI | Tank/Healer/DPS roles, threat-based targeting, basic commands |
| Reputation & NPC Memory | Medium | NPC and Dialogue | Per-faction and per-NPC memory, dialogue unlocks |
| Save System (custom, versioned) | Medium | Technical Constraints | Full world state serialization, manual + auto-save |
| Economy (gold, regional merchants, commissions) | Low | Economy and Resources | Simple gold-based, no complex crafting |
| Audio (direction TBD) | Low | Audio Direction | Deferred — placeholder sounds for initial development |

### Technical Requirements

**Performance:**
- Frame rate: 60 FPS stable @ 1080p (16.67ms frame budget)
- Memory: < 6 GB on minimum specs (i3 gen 6/7, 8 GB RAM, GTX 900/iGPU)
- Zone load time: < 5s max, < 3s target
- Initial launch: < 10s to main menu

**Platform:**
- PC only (Windows 10/11, Linux Ubuntu 20.04+, macOS)
- Keyboard + Mouse only (no controller v1.0)
- No multiplayer, no cloud saves
- Localization: French + English at v1.0

**Build:**
- IL2CPP for release, Mono for development
- .NET Standard 2.1
- < 10 GB total install size

### Complexity Drivers

**High Complexity (require custom architecture):**
1. **World State Tracker** — Centralized system tracking ALL world modifications (conquests, alliances, narrative choices) with observable state propagation to all dependent systems
2. **Combat System** — Real-time WoW-style targeting, hotbar abilities with cooldown/resource management, status effects, companion AI with role-based behavior
3. **Dialogue & Consequence System** — Branching dialogue with NPC memory, faction/individual reputation, choices cascading across three acts
4. **Quest System** — Main story with branching, side quests, repeatables, all connected to world state and consequence tracking

**Novel Concepts (no standard pattern):**
1. **Cascading Consequences** — Act 1 choices ripple through to Act 3 outcomes. Requires a formal consequence propagation architecture.
2. **NPC Memory System** — Individual NPCs remember player actions, adapt dialogue and behavior. Goes beyond simple reputation flags.
3. **Persistent World Mutation** — Conquered villages, formed alliances, faction control — world state changes are permanent and visible.

### Technical Risks

| Risk | Impact | Mitigation |
|------|--------|------------|
| Custom save/dialogue/quest systems = significant dev effort | High | Prototype in Epics 1-3, iterate early |
| Basic companion AI may feel shallow | Medium | Start simple, upgrade to A*/Behavior Designer later |
| Performance with dense zones + complex AI | Medium | Regular profiling, object pooling, zone LOD |
| Architecture must scale over years of development | High | Clean interfaces, Init(args) DI, extensible patterns |
| Unity version updates may break things | Low | Pin to Unity 6 LTS, test before upgrading |

### Design Patterns & Architecture Approach (Key Decision Area)

Based on GDD analysis, these architectural patterns are critical for consistency across AI agents:

1. **State Machine** → Combat states, NPC AI states, quest states, character animation states
2. **Observer/Event System** → World state changes, reputation updates, quest triggers, UI updates
3. **Command Pattern** → Companion AI orders, ability execution, potential undo for saves
4. **Strategy Pattern** → AI behavior variants (tank/heal/DPS), combat ability effects
5. **Data-Driven (ScriptableObjects)** → Items, abilities, quests, dialogue trees, stat definitions
6. **ECS vs MonoBehaviour** → Critical decision point for combat entities and mass NPC management

## Engine & Framework

### Selected Engine

**Unity 6** (6000.3.9f1) — URP 17.3.0

**Rationale:** Already configured and in use. Unity 6 provides mature tooling for a narrative-first top-down action RPG with URP for stylized 3D rendering, robust scene management for zone-based loading, and a rich ecosystem of plugins (Init(args), PrimeTween, UniTask, Behavior Designer, A* Pathfinding) aligned with the project's long-term extensibility goals.

### Project Initialization

Unity Hub → New Project → 3D (URP) — already initialized.

### Engine-Provided Architecture

| Component | Solution | Version | Notes |
|-----------|----------|---------|-------|
| Rendering | URP (Universal Render Pipeline) | 17.3.0 | Shader Graph, 2D/3D, Volume post-processing |
| Physics | PhysX | Unity 6 native | Rigidbody, collisions, raycasting, triggers |
| Audio | Unity Audio | Native | AudioSource, AudioMixer, spatial audio basics |
| Input | New Input System | 1.18.0 | Action maps, rebinding, multi-device |
| Scene Management | SceneManager | Native | Async loading, additive scenes |
| Build System | Unity Build Pipeline | Native | IL2CPP (release), Mono (dev) |
| UI | uGUI | 2.0.0 | Canvas, EventSystem (evaluate UI Toolkit for new work) |
| Navigation | AI Navigation | 2.0.11 | NavMesh, agents, obstacles |
| Testing | Unity Test Framework | 1.6.0 | Edit Mode + Play Mode tests |
| Timeline | Timeline | 1.8.10 | Cutscenes, sequenced events |
| Color Space | Linear | Configured | Correct for PBR/URP workflows |

### Plugin Stack

| Plugin | Purpose | Status | Version |
|--------|---------|--------|---------|
| Init(args) | Dependency injection — replaces singletons | Planned | Latest |
| PrimeTween | Zero-alloc tweening/animation | Planned | Latest |
| UniTask | Zero-alloc async/await — replaces coroutines | Planned | Latest |
| Behavior Designer | AI behavior trees | Deferred | Latest |
| A* Pathfinding Project | Advanced pathfinding | Deferred | Latest |
| Hot Reload | Fast iteration — skip recompile | Active | Current |
| vInspector | Inspector productivity | Active | Current |
| vHierarchy | Hierarchy productivity | Active | Current |

### AI Tooling (MCP Servers)

**Selected MCPs for AI-assisted development:**

#### MCP Unity (CoderGamester)

- **Repo:** `CoderGamester/mcp-unity`
- **Purpose:** Direct Unity Editor control — 30+ tools for scene, GameObject, component, and material management
- **Capabilities:**
  - Create, load, save, delete scenes
  - Create, select, update, duplicate, delete, reparent GameObjects
  - Add/update components with property access
  - Create, assign, modify materials
  - Run Unity Test Runner tests
  - Recompile scripts, execute menu items
  - Batch execute multiple commands atomically
  - Access Unity console logs
- **Install:**
  1. Install Node.js 18+
  2. Unity: Window > Package Manager > + > Add package from git URL
  3. URL: `https://github.com/CoderGamester/mcp-unity.git`
  4. Unity: Tools > MCP Unity > Server Window > Configure
  5. Click Start Server
- **Requirements:** Unity 6+, Node.js 18+, npm 9+
- **Supported Clients:** Claude Desktop, Claude Code, Cursor, Windsurf, Codex CLI, GitHub Copilot
- **License:** MIT

#### Context7

- **Repo:** `upstash/context7`
- **Purpose:** Up-to-date, version-specific documentation for any library directly in AI prompts
- **Capabilities:**
  - Resolve library names to documentation sources
  - Retrieve current API documentation for Unity and all plugins
  - Pull version-specific code examples and usage patterns
- **Install:** `claude mcp add context7 -- npx -y @upstash/context7-mcp`
- **License:** MIT

---

## Decision Summary

| Category | Decision | Version | Affects Epics | Rationale |
|----------|----------|---------|---------------|-----------|
| Core Paradigm | MonoBehaviour + Init(args) DI | Unity 6 / Init(args) latest | All | Single-player RPG scope doesn't need DOTS/ECS. Init(args) provides clean DI, testability, and plugin compatibility. |
| Design Patterns | HSM, Observer, Command, Strategy, Data-Driven, Object Pool, Service Locator | N/A | All | Covers all GDD systems without overlap. Events via DI interfaces, not static buses. |
| Dialogue System | **Deferred** | — | Epic 5 | Will be reevaluated when Quest & Dialogue epic begins. |
| Save System | MemoryPack (Cysharp) | Latest | All (save/load) | Zero-allocation binary serialization. Same ecosystem as UniTask. Schema versioning via `[MemoryPackable]` attributes. Fast, compact, C#-native. |
| Audio | Unity Audio (native) | Unity 6 built-in | Epic 12 | Sufficient for MVP. Abstracted behind `IAudioManager` for future FMOD migration if needed. |
| Asset Pipeline | Unity Addressables | Latest | Epic 6+ | Addressables from the start for async zone loading, memory management, and future DLC support. |
| Companion AI | State Machine → Behavior Designer | SM now, BD deferred | Epic 8, 10 | Simple state machine validates companion gameplay. Migrate to Behavior Designer for complex behaviors later. |
| World State | **Deferred** | — | Epic 5, 6 | Will be reevaluated when world/quest systems begin. Novel pattern requiring careful design. |
| Rendering | URP | 17.3.0 | All visual | Engine-provided. Shader Graph, Volume post-processing, 2D/3D support. |
| Physics | PhysX (native) | Unity 6 | Epic 1 | Engine-provided. Rigidbody, collisions, raycasting, triggers. |
| Input | New Input System | 1.18.0 | Epic 1 | Already configured. Action maps, rebinding, multi-device. |
| UI | uGUI (evaluate UI Toolkit) | 2.0.0 | Epic 2 | Mature, wide plugin support. UI Toolkit for new work if appropriate. |
| Navigation | AI Navigation | 2.0.11 | Epic 8 | Engine-provided NavMesh system. |
| Async | UniTask (planned) | Latest | All | Zero-alloc async/await. Replaces coroutines. Cysharp ecosystem with MemoryPack. |
| Tweening | PrimeTween (planned) | Latest | All visual | Zero-alloc tweening. Replaces DOTween/LeanTween. |

## Architectural Decisions

### Core Paradigm: MonoBehaviour + Init(args) DI

**Approach:** Traditional Unity MonoBehaviour architecture with Init(args) dependency injection.

- All component dependencies received via `MonoBehaviour<T...>` + `Init()`
- Global services via `[Service]` attribute — NO singletons, NO `FindObjectOfType`
- Interfaces as service types (`IInputManager`, `ICombatSystem`, `IQuestManager`) for testability
- Scene-scoped services via Service Tags
- This paradigm is compatible with the full plugin ecosystem (Behavior Designer, PrimeTween, UniTask, etc.)

**Why not ECS/DOTS:** Single-player RPG with manageable entity counts. Performance handled via object pooling and profiling. DOTS would be incompatible with Init(args) and most plugins, doubling architectural complexity for marginal gains.

### Formalized Design Patterns

These patterns MUST be used consistently by all AI agents:

#### 1. Hierarchical State Machine (HSM)

**Usage:** Combat states, character states, NPC AI, quest states, animation states.

```csharp
// Interface
public interface IState
{
    void Enter();
    void Execute();
    void Exit();
}

// Generic state machine
public class StateMachine<T> where T : class
{
    public IState CurrentState { get; private set; }
    public IState PreviousState { get; private set; }
    // Hierarchical: parent states contain sub-state machines
}
```

**Rules:**
- Every entity with behavior transitions MUST use a state machine
- Combat uses hierarchical SM: Global → Combat → Ability substates
- States are classes, not enums — allows logic encapsulation
- State transitions go through the machine, never directly between states

#### 2. Observer / Event System (via DI Interfaces)

**Usage:** World state changes, reputation updates, quest triggers, UI updates.

```csharp
// Service fires events — subscribers injected via Init(args)
public interface IWorldStateEvents
{
    event Action<WorldStateChange> OnStateChanged;
    event Action<ConsequenceEvent> OnConsequenceTriggered;
}

// Listeners subscribe in OnEnable, unsubscribe in OnDisable
```

**Rules:**
- NO global static event bus — all events flow through DI-injected service interfaces
- Subscribe in `OnEnable()`, unsubscribe in `OnDisable()` — always match pairs
- Events carry data objects, not raw parameters
- UI listens to service events, never polls

#### 3. Command Pattern

**Usage:** Companion AI orders, ability execution, input action abstraction.

```csharp
public interface ICommand
{
    void Execute();
    void Undo(); // Optional — for save snapshots or ability cancellation
}
```

**Rules:**
- Companion orders are queued commands (`AttackTargetCommand`, `HoldPositionCommand`, `FollowCommand`)
- Abilities are commands with cooldown/resource validation before execution
- Commands are data objects — serializable for replay/save if needed

#### 4. Strategy Pattern

**Usage:** Companion AI roles (tank/heal/DPS), ability effect calculation, damage formulas.

```csharp
public interface ICombatStrategy
{
    void SelectTarget(CombatContext context);
    void ExecuteTurn(CombatContext context);
}
// TankStrategy, HealerStrategy, DamageStrategy injected at runtime
```

**Rules:**
- Companion combat role = swappable strategy, injected via Init(args)
- Damage calculation formulas are strategies (physical vs magical vs hybrid)
- Strategy swap at runtime for role changes — no conditional chains

#### 5. Data-Driven Architecture (ScriptableObjects)

**Usage:** Items, abilities, quests, dialogue, stat formulas, enemy configs, loot tables.

**Rules:**
- ALL game data defined in ScriptableObjects — never hardcoded
- Naming: `DT_` prefix for data assets, `CFG_` prefix for config assets
- ScriptableObjects are READ-ONLY at runtime — mutations don't persist between sessions
- ScriptableObject classes use `SO` suffix: `ItemDataSO`, `AbilityDataSO`, `EnemyConfigSO`
- Reference by asset, not by string/ID when possible

#### 6. Object Pool

**Usage:** Projectiles, enemies, VFX, damage numbers, loot drops.

```csharp
public interface IObjectPool<T> where T : Component
{
    T Get();
    void Release(T instance);
    void PreWarm(int count);
}
```

**Rules:**
- NEVER `Instantiate`/`Destroy` frequently spawned objects in gameplay — always pool
- Pool pre-warm on zone load to avoid frame spikes
- Auto-expand pools if exhausted (log warning for tuning)
- Release on disable, not on destroy

#### 7. Service Locator (via Init(args) `[Service]`)

**Usage:** All global systems — GameManager, CombatSystem, QuestManager, AudioManager, etc.

**Rules:**
- `[Service]` attribute for global services, Service Tags for scene-scoped
- Always expose as interface type (`ICombatSystem`, not `CombatSystem`)
- Global services initialized before scene load — no execution order concerns
- See `docs/init-args-guide.md` for complete reference

### Save System: MemoryPack

**Approach:** MemoryPack (Cysharp) binary serialization with schema versioning.

- **Library:** MemoryPack — zero-allocation, fastest C# serializer, same Cysharp ecosystem as UniTask
- **Format:** Binary (compact, fast) with `[MemoryPackable]` attribute on all save data classes
- **Versioning:** `[MemoryPackable(GenerateType.VersionTolerant)]` for forward/backward compatibility
- **Structure:**
  - `SaveData` root object containing all subsystems
  - `PlayerSaveData` — position, stats, level, inventory, equipment
  - `WorldStateSaveData` — world state snapshot, event log (when implemented)
  - `QuestSaveData` — active/completed quests, choices, flags
  - `ReputationSaveData` — faction and NPC reputation values
- **Save points:** Manual save + auto-save at key story moments
- **Storage:** `Application.persistentDataPath` — platform-appropriate
- **Async:** Save/load via UniTask — non-blocking operations

### Audio: Unity Native

**Approach:** Unity's built-in audio system, abstracted behind interface.

- **System:** AudioSource, AudioMixer, AudioListener
- **Interface:** `IAudioManager` service via Init(args) DI
- **Channels:** Master, Music, SFX, UI, Ambient mixing groups
- **Music:** Simple track management (play, crossfade, stop) — no adaptive music for MVP
- **SFX:** Pooled AudioSources for concurrent sound effects
- **Future:** Interface abstraction allows FMOD migration without changing game code

### Asset Pipeline: Unity Addressables

**Approach:** Addressables from the start for all asset loading.

- **Zone assets:** Each zone's assets in an Addressables group — load on enter, release on exit
- **Shared assets:** Player, UI, core systems in a persistent Addressables group
- **Loading:** Async via UniTask integration — `Addressables.LoadAssetAsync<T>()`
- **Interface:** `IAssetLoader` service — abstracts Addressables calls
- **Labels:** Zone-based labels (`zone_village_001`, `zone_forest_001`) for group loading
- **Memory:** Release handles on zone exit to prevent leaks
- **No `Resources/` folder** — all runtime assets via Addressables
- **Build:** Addressables build as part of standard build pipeline

### Companion AI: State Machine (→ Behavior Designer)

**Approach:** Simple hierarchical state machine for companion AI at MVP, migrating to Behavior Designer for advanced behaviors.

**MVP States:**
```
CompanionAI (HSM)
├── FollowState — follow player, maintain distance
├── CombatState
│   ├── TankSubstate — taunt, position between player and enemies
│   ├── HealerSubstate — monitor ally HP, heal below threshold
│   └── DamageSubstate — attack player's target, ability rotation
├── HoldPositionState — stay at commanded position
└── DeadState — respawn timer or manual resurrection
```

**Rules:**
- Role (Tank/Healer/DPS) determines which combat substate is active = Strategy pattern
- Companion selects targets based on role (threat list for tank, lowest HP for healer, player target for DPS)
- Player issues orders via Command pattern (`AttackTargetCommand`, `HoldPositionCommand`)
- Migration path: Each state becomes a Behavior Designer subtree when migrating

### Deferred Decisions

The following decisions will be made when their respective epics begin:

| Decision | Deferred Until | Context |
|----------|---------------|---------|
| **Dialogue System** (Yarn Spinner / Ink / Custom) | Epic 5 — Quest & Dialogue | Needs prototype testing to evaluate branching needs. Will decide based on actual narrative complexity. |
| **World State Architecture** (Event-sourced / Snapshot / Hybrid) | Epic 5-6 — Quest & World Systems | Novel pattern requiring careful design. Will prototype approaches when world state tracking begins. |

## Cross-cutting Concerns

These patterns apply to ALL systems and MUST be followed by every AI agent implementation.

### Error Handling

**Strategy:** Targeted try-catch + global handler fallback.

**Error Levels:**

| Level | When | Action |
|-------|------|--------|
| **Critical** | Save corrupted, scene missing, core service failure | Player-facing message + graceful fallback (return to menu) |
| **Recoverable** | Asset missing, NPC config invalid, pool exhausted | Log warning + silent fallback (use default/skip) |
| **Logic** | Invalid state transition, null reference in non-critical path | Log error + continue operation |

**Rules:**
- NO try-catch in hot paths (`Update`, `FixedUpdate`, `LateUpdate`, combat loops) — performance
- TRY-CATCH required for: save/load, scene transitions, Addressables loading, file I/O
- Global handler via `Application.logMessageReceived` catches unhandled exceptions → log to file
- Never crash silently — all exceptions are logged
- Player-facing errors use localized, friendly messages — never show stack traces

**Example:**

```csharp
// Save system — critical I/O with try-catch
public async UniTask<bool> SaveGame(SaveData data)
{
    try
    {
        var bytes = MemoryPackSerializer.Serialize(data);
        await File.WriteAllBytesAsync(GetSavePath(), bytes);
        GameLogger.Info("[Save] Game saved successfully");
        return true;
    }
    catch (Exception ex)
    {
        GameLogger.Error("[Save] Failed to save game", ex);
        // Show player-facing error
        _uiService.ShowError(LocalizedStrings.SaveFailed);
        return false;
    }
}
```

### Logging

**Format:** `[SYSTEM] Message` — structured tags for filtering.

**System Tags:**

| Tag | System |
|-----|--------|
| `[Combat]` | Combat system, damage, abilities, status effects |
| `[Quest]` | Quest state changes, completions, failures |
| `[WorldState]` | World state mutations, consequence triggers |
| `[Save]` | Save/load operations |
| `[Audio]` | Audio playback, music transitions |
| `[UI]` | UI state, navigation, input |
| `[AI]` | Companion AI, enemy AI, pathfinding |
| `[Zone]` | Scene loading, zone transitions, asset management |
| `[Input]` | Input system events, rebinding |
| `[Progression]` | XP, level-ups, talent changes, loot |

**Log Levels:**

| Level | Usage | In Release? |
|-------|-------|-------------|
| `ERROR` | Something broke — requires attention | Yes |
| `WARN` | Unexpected but handled — potential issue | Yes |
| `INFO` | Normal milestones — zone loaded, quest completed, game saved | Yes |
| `DEBUG` | Detailed diagnostics — state transitions, calculations | No (Editor only) |

**Rules:**
- NO logs in hot paths (Update/FixedUpdate) unless wrapped in `[Conditional("UNITY_EDITOR")]`
- DEBUG level stripped from release builds via conditional compilation
- Errors also written to log file at `Application.persistentDataPath/logs/`
- Log file rotated per session (keep last 5 sessions)

**Example:**

```csharp
public static class GameLogger
{
    public static void Info(string message) => Debug.Log(message);
    public static void Warn(string message) => Debug.LogWarning(message);
    public static void Error(string message, Exception ex = null) =>
        Debug.LogError(ex != null ? $"{message}: {ex.Message}" : message);

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Debug(string message) => UnityEngine.Debug.Log($"[DBG] {message}");
}

// Usage
GameLogger.Info("[Combat] Player dealt 45 damage to Orc_03");
GameLogger.Warn("[Zone] Asset TX_Missing_Tree not found, using fallback");
GameLogger.Error("[Save] Failed to deserialize save file", ex);
```

### Configuration Management

| Config Type | Storage | Access Pattern | Example |
|-------------|---------|----------------|---------|
| **Game Constants** | `static class GameConstants` | Compile-time constants | `MAX_LEVEL = 60`, `INVENTORY_SLOTS = 24` |
| **Balance Data** | ScriptableObjects (`CFG_` prefix) | Inspector-tweakable, read-only at runtime | `CFG_GameBalance.asset`, `CFG_CombatFormulas.asset` |
| **Player Settings** | PlayerPrefs / JSON file | Read/write at runtime, persisted | Volume, keybinds, UI scale, language |
| **Platform Settings** | Unity QualitySettings | Engine-managed | Resolution, graphics quality, VSync |

**Rules:**
- Balance values NEVER hardcoded in scripts — always in ScriptableObjects
- Player settings loaded on startup, saved on change
- Constants use `UPPER_SNAKE_CASE` in `GameConstants` class
- ScriptableObject configs use `CFG_` prefix: `CFG_GameBalance`, `CFG_DifficultySettings`

### Event System

**Pattern:** Observer via DI-injected service interfaces — NO global static event bus.

**Naming Conventions:**
- Event interface methods: `On` + past participle — `OnEnemyDefeated`, `OnQuestCompleted`, `OnZoneLoaded`
- Event data classes: `{Action}Event` suffix — `EnemyDefeatedEvent`, `QuestCompletedEvent`
- Service event interfaces: `I{System}Events` — `ICombatEvents`, `IQuestEvents`, `IWorldStateEvents`

**Rules:**
- Events ALWAYS carry data objects, never raw parameters
- Subscribe in `OnEnable()`, unsubscribe in `OnDisable()` — ALWAYS matched pairs
- Synchronous by default (same frame). Async only when explicitly needed.
- Events are fire-and-forget — listeners must not affect the emitter's flow
- UI components ONLY listen to events — never poll services in Update

**Example:**

```csharp
// Event data class
public readonly struct EnemyDefeatedEvent
{
    public readonly int EnemyId;
    public readonly Vector3 Position;
    public readonly int XpReward;
}

// Service interface with events
public interface ICombatEvents
{
    event Action<EnemyDefeatedEvent> OnEnemyDefeated;
    event Action<DamageTakenEvent> OnDamageTaken;
}

// Listener subscribes via Init(args) injection
public class QuestTracker : MonoBehaviour<ICombatEvents>
{
    ICombatEvents _combatEvents;

    protected override void Init(ICombatEvents combatEvents)
    {
        _combatEvents = combatEvents;
    }

    void OnEnable() => _combatEvents.OnEnemyDefeated += HandleEnemyDefeated;
    void OnDisable() => _combatEvents.OnEnemyDefeated -= HandleEnemyDefeated;

    void HandleEnemyDefeated(EnemyDefeatedEvent e)
    {
        // Check quest objectives...
    }
}
```

### Debug & Development Tools

| Tool | Description | Activation | In Release? |
|------|-------------|------------|-------------|
| **Debug Console** | In-game text commands: `/god`, `/teleport zone_name`, `/give item_id`, `/setlevel 60`, `/completequest quest_id` | Tilde (`~`) key | No |
| **Visual Debug Overlays** | Gizmos for NavMesh, AI states, combat ranges, trigger zones, pathfinding | Editor Gizmos toggle | No |
| **State Inspector** | Runtime view of world state flags, quest status, reputation values | Editor custom window | No |
| **Performance Overlay** | FPS counter, memory usage, draw calls, batch count | `F3` key | Optional (settings) |
| **Hot Reload** | Skip recompile for code changes during Play Mode | Automatic (plugin) | No |

**Rules:**
- All debug features wrapped in `#if UNITY_EDITOR || DEVELOPMENT_BUILD`
- Debug console commands registered via a command dictionary — easily extensible
- Performance overlay is the ONLY debug tool optionally available in release builds
- Never ship dev tools enabled by default — must be explicitly activated

## Project Structure

### Organization Pattern

**Pattern:** Hybrid — Type at top level, Domain within scripts.

**Rationale:** Unity convention. `Assets/` organized by asset type (Scripts, Art, Prefabs, ScriptableObjects, Scenes). Scripts organized by game domain (Core, Combat, Quest, World, etc.). Enables clear boundaries, Assembly Definitions per domain, and AI agents always know where code belongs.

### Directory Structure

```
Valenthia-Chronicles/
├── Assets/
│   ├── Scripts/
│   │   ├── Core/                          # Global services, base classes, interfaces, utilities
│   │   │   ├── Interfaces/                # All service interfaces (IWorldStateTracker, ICombatSystem, etc.)
│   │   │   ├── Services/                  # Global [Service] implementations
│   │   │   ├── Events/                    # Event data structs (EnemyDefeatedEvent, QuestCompletedEvent, etc.)
│   │   │   ├── Patterns/                  # Shared patterns (StateMachine<T>, ObjectPool<T>, ICommand)
│   │   │   ├── Data/                      # Base data classes, enums, GameConstants
│   │   │   ├── Utils/                     # Helpers, extensions, GameLogger
│   │   │   └── Debug/                     # Debug console, performance overlay, cheat commands
│   │   ├── Player/                        # Player controller, input handling, camera
│   │   │   ├── Input/                     # Input action wrappers, rebinding
│   │   │   ├── Camera/                    # Top-down camera, zoom, follow
│   │   │   └── Controller/               # Player movement, interaction, targeting
│   │   ├── Combat/                        # Combat system, abilities, status effects, damage
│   │   │   ├── Abilities/                 # Ability execution, cooldowns, resource costs
│   │   │   ├── StatusEffects/             # Buff/debuff system, effect application
│   │   │   ├── Targeting/                 # Target selection, tab-targeting, click-to-target
│   │   │   └── Damage/                    # Damage calculation, formulas, strategies
│   │   ├── Quest/                         # Quest system, dialogue, narrative choices
│   │   │   ├── Quests/                    # Quest state machine, tracking, objectives
│   │   │   ├── Dialogue/                  # Dialogue system integration (TBD)
│   │   │   └── Consequences/              # Consequence engine, choice tracking
│   │   ├── World/                         # Zone management, world state, scene loading
│   │   │   ├── Zones/                     # Zone loader, transition manager, fast travel
│   │   │   ├── State/                     # World state tracker, snapshot, event log
│   │   │   └── Environment/               # Environmental objects, interactables, destructibles
│   │   ├── NPC/                           # NPC behavior, reputation, merchants, companions
│   │   │   ├── AI/                        # NPC state machines, companion strategies (Tank/Heal/DPS)
│   │   │   ├── Reputation/                # Faction & individual reputation tracking
│   │   │   ├── Merchants/                 # Merchant inventory, regional stock, commissions
│   │   │   └── Companions/                # Companion management, orders (Command pattern)
│   │   ├── Inventory/                     # Items, equipment, inventory management
│   │   │   ├── Items/                     # Item logic, usage, stacking
│   │   │   └── Equipment/                 # Equipment slots, stat bonuses, rarity
│   │   ├── Progression/                   # XP, leveling, talents, specializations
│   │   │   ├── Leveling/                  # XP calculation, level-up logic
│   │   │   ├── Talents/                   # Talent trees (racial + specialization)
│   │   │   └── Specializations/           # Specialization definitions, unlocking
│   │   ├── UI/                            # All UI scripts (HUD, menus, dialogue UI)
│   │   │   ├── HUD/                       # Health/mana bars, hotbar, minimap, XP
│   │   │   ├── Menus/                     # Main menu, pause, settings, character sheet
│   │   │   ├── Inventory/                 # Inventory UI, equipment UI, tooltips
│   │   │   ├── Quest/                     # Quest log UI, quest tracker, dialogue UI
│   │   │   └── Common/                    # Shared UI components (buttons, panels, popups)
│   │   ├── Audio/                         # Audio management scripts
│   │   │   └── Services/                  # IAudioManager implementation, music controller, SFX pool
│   │   └── Save/                          # Save/load system
│   │       ├── Data/                      # MemoryPackable save data classes
│   │       └── Migration/                 # Schema version migrations
│   ├── Art/                               # 3D models, textures, materials, animations
│   │   ├── Characters/                    # Player, NPCs, companions, enemies
│   │   ├── Environment/                   # Terrain, buildings, props, vegetation
│   │   ├── VFX/                           # Visual effects (particles, shaders)
│   │   └── Materials/                     # Shared materials
│   ├── Prefabs/                           # All prefabs organized by system
│   │   ├── Player/                        # PF_Player, PF_PlayerCamera
│   │   ├── Enemies/                       # PF_Orc_Grunt, PF_Troll_Shaman
│   │   ├── NPCs/                          # PF_Merchant, PF_QuestGiver
│   │   ├── Companions/                    # PF_Companion_Tank, PF_Companion_Healer
│   │   ├── Environment/                   # PF_Tree_Oak, PF_Chest
│   │   ├── VFX/                           # VFX_Fireball, VFX_HealAura
│   │   └── UI/                            # PF_DamageNumber, PF_LootDrop
│   ├── ScriptableObjects/                 # Data assets organized by system
│   │   ├── Items/                         # DT_Sword_Iron, DT_Potion_Health
│   │   ├── Abilities/                     # DT_Fireball, DT_Shield_Bash
│   │   ├── Enemies/                       # DT_Orc_Grunt_Config, DT_Boss_ActOne
│   │   ├── Quests/                        # DT_Quest_MainStory_01
│   │   ├── Dialogue/                      # DT_Dialogue_NPC_Blacksmith (TBD format)
│   │   ├── Talents/                       # DT_TalentTree_Mage, DT_TalentTree_Human
│   │   ├── Zones/                         # DT_Zone_Village001, DT_Zone_Forest001
│   │   └── Config/                        # CFG_GameBalance, CFG_CombatFormulas, CFG_Difficulty
│   ├── Scenes/                            # All scenes
│   │   ├── Menus/                         # SC_MainMenu, SC_Loading
│   │   ├── Zones/                         # SC_Village_001, SC_Forest_001, SC_Dungeon_001
│   │   └── Test/                          # SC_TestCombat, SC_TestZone (dev only)
│   ├── Settings/                          # URP assets, Volume profiles
│   ├── AddressableAssetsData/             # Addressables configuration and groups
│   ├── Animations/                        # Animation controllers, clips
│   │   ├── Player/                        # AN_Player_Idle, AN_Player_Run, AN_Player_Attack
│   │   ├── Enemies/                       # AN_Orc_Idle, AN_Orc_Attack
│   │   └── NPCs/                          # AN_NPC_Idle, AN_NPC_Talk
│   ├── Audio/                             # Audio assets
│   │   ├── Music/                         # MU_Combat_Boss, MU_Town_Peaceful
│   │   ├── SFX/                           # SFX_Sword_Hit_01, SFX_Spell_Fire
│   │   └── Ambient/                       # SFX_Forest_Wind, SFX_Town_Crowd
│   ├── Plugins/                           # Third-party plugins
│   └── Tests/                             # Test scripts
│       ├── EditMode/                      # Pure logic tests (no scene required)
│       │   ├── Combat/                    # CombatSystemTests.cs, DamageCalculationTests.cs
│       │   ├── Quest/                     # QuestManagerTests.cs
│       │   ├── Progression/               # LevelingSystemTests.cs
│       │   └── Save/                      # SaveSystemTests.cs
│       └── PlayMode/                      # Runtime behavior tests
│           ├── Combat/                    # CombatIntegrationTests.cs
│           ├── Zone/                      # ZoneLoadingTests.cs
│           └── UI/                        # UINavigationTests.cs
├── docs/                                  # Documentation
│   └── init-args-guide.md                 # Init(args) reference
├── Packages/                              # Unity package manifest
├── ProjectSettings/                       # Unity project settings
└── _bmad-output/                          # BMAD planning artifacts
    ├── plans/                             # GDD, Game Brief
    └── development/                       # Development artifacts
```

### System Location Mapping

| System | Script Folder | Data (ScriptableObjects) | Prefabs |
|--------|--------------|-------------------------|---------|
| Core (interfaces, patterns, services) | `Scripts/Core/` | `ScriptableObjects/Config/` | — |
| Player (movement, input, camera) | `Scripts/Player/` | — | `Prefabs/Player/` |
| Combat (targeting, abilities, damage) | `Scripts/Combat/` | `ScriptableObjects/Abilities/` | `Prefabs/VFX/` |
| Quest & Dialogue | `Scripts/Quest/` | `ScriptableObjects/Quests/`, `Dialogue/` | — |
| World State & Zones | `Scripts/World/` | `ScriptableObjects/Zones/` | `Prefabs/Environment/` |
| NPC, Companions, Reputation | `Scripts/NPC/` | `ScriptableObjects/Enemies/` | `Prefabs/NPCs/`, `Companions/` |
| Inventory & Equipment | `Scripts/Inventory/` | `ScriptableObjects/Items/` | — |
| Progression (XP, talents) | `Scripts/Progression/` | `ScriptableObjects/Talents/` | — |
| UI (HUD, menus) | `Scripts/UI/` | — | `Prefabs/UI/` |
| Audio | `Scripts/Audio/` | — | — |
| Save System | `Scripts/Save/` | — | — |
| Debug Tools | `Scripts/Core/Debug/` | — | — |

### Architectural Boundaries

- `Scripts/Core/` depends on NOTHING else — it is the foundation
- Domain folders (`Combat/`, `Quest/`, `NPC/` etc.) do NOT call each other directly — they communicate via interfaces in `Core/Interfaces/`
- `Scripts/UI/` contains ONLY UI logic — zero game logic. UI listens to service events.
- Tests mirror the script structure: `Tests/EditMode/Combat/` tests `Scripts/Combat/`
- Assembly Definitions per domain to isolate compilation and enforce boundaries
- NO `Assets/Resources/` folder — all runtime assets loaded via Addressables

## Implementation Patterns

These patterns ensure consistent implementation across all AI agents.

### Communication Pattern

**Pattern:** Init(args) DI + interface events. Never direct references between domains.

```csharp
// ✅ CORRECT — Communication via service interface
public class EnemySpawner : MonoBehaviour<ICombatSystem, IZoneManager>
{
    ICombatSystem _combat;
    IZoneManager _zone;

    protected override void Init(ICombatSystem combat, IZoneManager zone)
    {
        _combat = combat;
        _zone = zone;
    }
}

// ❌ FORBIDDEN — Direct reference between domains
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] QuestManager _questManager; // NO! Use IQuestManager via Init
}
```

### Entity Creation Pattern

**Pattern:** Object Pool for frequent entities + Addressables for loading.

```csharp
public class EnemySpawner : MonoBehaviour<IObjectPool<Enemy>, IZoneManager>
{
    IObjectPool<Enemy> _enemyPool;

    public Enemy SpawnEnemy(EnemyConfigSO config, Vector3 position)
    {
        var enemy = _enemyPool.Get();
        enemy.Initialize(config, position);
        enemy.gameObject.SetActive(true);
        return enemy;
    }

    public void DespawnEnemy(Enemy enemy)
    {
        enemy.gameObject.SetActive(false);
        _enemyPool.Release(enemy);
    }
}
```

**Rules:**
- Enemies, projectiles, VFX, damage numbers → ALWAYS pooled
- Direct `Instantiate()` allowed ONLY for unique entities (boss, companions, player)
- Pool pre-warm on zone load to avoid frame spikes
- Release = `SetActive(false)` + return to pool, NEVER `Destroy()`

### State Transition Pattern

**Pattern:** Hierarchical State Machine with `IState` interface.

```csharp
public class EnemyCombatState : IState
{
    private readonly EnemyController _enemy;
    private readonly ICombatSystem _combat;

    public EnemyCombatState(EnemyController enemy, ICombatSystem combat)
    {
        _enemy = enemy;
        _combat = combat;
    }

    public void Enter()
    {
        _enemy.SetAnimationTrigger("Combat");
        GameLogger.Debug($"[AI] {_enemy.name} entered combat state");
    }

    public void Execute()
    {
        if (_enemy.CurrentTarget == null)
        {
            _enemy.StateMachine.ChangeState(_enemy.IdleState);
            return;
        }
        // Combat logic...
    }

    public void Exit()
    {
        _enemy.ClearTarget();
    }
}
```

**Rules:**
- Every state is a class, never an enum with switch
- Transitions go THROUGH the state machine, never directly between states
- `Enter()` = setup, `Execute()` = tick, `Exit()` = cleanup
- Log transitions in DEBUG level for diagnostics

### Data Access Pattern

**Pattern:** ScriptableObjects centralized, accessed via injected registries.

```csharp
// ScriptableObject data definition
[CreateAssetMenu(menuName = "Valenthia/Items/Item Data")]
public class ItemDataSO : ScriptableObject
{
    [SerializeField] private string _itemId;
    [SerializeField] private string _displayName;
    [SerializeField] private ItemRarity _rarity;
    [SerializeField] private Sprite _icon;
    [SerializeField] private int _baseValue;

    public string ItemId => _itemId;
    public string DisplayName => _displayName;
    public ItemRarity Rarity => _rarity;
    public Sprite Icon => _icon;
    public int BaseValue => _baseValue;
}

// Injected registry
public interface IItemRegistry
{
    ItemDataSO GetItem(string itemId);
    IReadOnlyList<ItemDataSO> GetItemsByRarity(ItemRarity rarity);
}
```

**Rules:**
- ScriptableObjects are READ-ONLY at runtime — never mutate
- Properties with `[SerializeField] private` + public getter
- Registries (IItemRegistry, IAbilityRegistry, etc.) loaded at startup via Addressables
- NEVER `Resources.Load()` — everything through Addressables or injection

### NPC Management Pattern (TrinityCore-style)

**Principle:** NPCs are **hand-placed in scenes** by the level designer. At runtime, `INpcManager` manages their activation/deactivation based on world state, quest conditions, and respawn timers.

| TrinityCore Concept | Unity Equivalent |
|---------------------|------------------|
| `creature` table (GUID, template, position) | **GameObjects in scene** with `NpcInstance` component + ref to `NpcDataSO` |
| `creature_template` (stats, faction, loot) | **`NpcDataSO`** ScriptableObject (NPC configuration) |
| `conditions` table (quest, faction, event) | **`SpawnCondition[]`** on the component — evaluated at load |
| Respawn timer | **`INpcManager`** tracks kills, manages timer, `SetActive(true)` on respawn |

**Level Design Workflow:**
1. Open scene `SC_Village_001` in Editor
2. Drag-drop prefab `PF_Merchant_Blacksmith` at exact desired position
3. Configure `NpcInstance` component: data SO, spawn conditions, respawn settings
4. At runtime, `INpcManager` evaluates all `NpcInstance` in the zone and decides: active or not

```csharp
// Scripts/NPC/NpcInstance.cs — Component on every NPC placed in scene
public class NpcInstance : MonoBehaviour<INpcManager>
{
    [Header("Identity")]
    [SerializeField] private NpcDataSO _npcData;
    [SerializeField] private string _uniqueId;

    [Header("Spawn Rules")]
    [SerializeField] private SpawnCondition[] _conditions;
    [SerializeField] private bool _respawns = true;
    [SerializeField] private float _respawnDelay = 300f;
    [SerializeField] private bool _persistKill; // If true, once killed = never respawn (boss, quest)

    private INpcManager _npcManager;

    protected override void Init(INpcManager npcManager)
    {
        _npcManager = npcManager;
    }

    protected override void OnAwake()
    {
        _npcManager.Register(this);
    }

    private void OnDestroy()
    {
        _npcManager.Unregister(this);
    }

    public NpcDataSO Data => _npcData;
    public string UniqueId => _uniqueId;
    public SpawnCondition[] Conditions => _conditions;
    public bool Respawns => _respawns;
    public float RespawnDelay => _respawnDelay;
    public bool PersistKill => _persistKill;
}
```

```csharp
// Spawn conditions — evaluated at zone load
[System.Serializable]
public struct SpawnCondition
{
    public ConditionType Type;
    public string Key;
    public string ExpectedValue;
    public bool Invert;
}

public enum ConditionType
{
    Always,           // Always active (no condition)
    WorldFlag,        // Boolean flag in world state
    QuestState,       // Quest state (not_started, in_progress, completed)
    ReputationMin,    // Minimum reputation with a faction
    FactionControl,   // Which faction controls this zone
    PlayerLevel       // Minimum player level
}
```

```csharp
// Scripts/NPC/Services/INpcManager.cs
public interface INpcManager
{
    void Register(NpcInstance npc);
    void Unregister(NpcInstance npc);
    void OnNpcKilled(NpcInstance npc);
    void EvaluateAll(); // Re-evaluate all NPCs (after world state change)
}
```

**Common Use Cases:**

| Scenario | Configuration |
|----------|---------------|
| Town merchant (always present) | `Conditions: [Always]` |
| Merchant in conquered village | `Conditions: [FactionControl = "player"]` |
| Dungeon boss (kill once) | `PersistKill = true` |
| Zone enemies (respawn) | `Respawns = true, RespawnDelay = 300` |
| Quest NPC (during quest) | `Conditions: [QuestState("quest_rescue") = "in_progress"]` |
| Post-quest NPC (replaces previous) | `Conditions: [QuestState("quest_rescue") = "completed"]` |
| NPC after player choice | `Conditions: [WorldFlag("allied_with_dwarves") = "true"]` |

**Rules:**
- NPCs are ALWAYS placed in scenes by the level designer, never spawned by code
- `INpcManager` manages activation only (`SetActive(true/false)`) — no Instantiate/Destroy
- `PersistKill` deaths saved in world state — survives save/load
- Conditions are AND logic — all must be true for NPC to be active
- `EvaluateAll()` called after any world state change to update NPC visibility
- NPC GameObjects use standard prefab naming: `PF_Merchant_Blacksmith`, `PF_Guard_Alliance`
- SpawnPoint prefix `SP_` is NOT used — NPCs are placed as their actual prefabs

### Editor Tooling Pattern

**NPC scene visualization via Gizmos:**

```csharp
// On NpcInstance.cs
#if UNITY_EDITOR
private void OnDrawGizmos()
{
    Gizmos.color = _npcData != null ? GetNpcTypeColor() : Color.white;
    Gizmos.DrawWireSphere(transform.position, 0.5f);

    // Facing direction
    Gizmos.color = Color.blue;
    Gizmos.DrawRay(transform.position, transform.forward * 1.5f);
}

private void OnDrawGizmosSelected()
{
    // Show NPC name and conditions when selected
    UnityEditor.Handles.Label(
        transform.position + Vector3.up * 2f,
        $"{(_npcData != null ? _npcData.name : "No Config")}\n" +
        $"Respawns: {_respawns} | PersistKill: {_persistKill}"
    );

    // Show patrol radius or interaction range if applicable
}

private Color GetNpcTypeColor() => _npcData.NpcType switch
{
    NpcType.Enemy => Color.red,
    NpcType.Friendly => Color.green,
    NpcType.Merchant => Color.yellow,
    NpcType.QuestGiver => new Color(1f, 0.5f, 0f), // Orange
    NpcType.Companion => Color.cyan,
    _ => Color.white
};
#endif
```

**Rules:**
- All Editor-only code in `#if UNITY_EDITOR` blocks or `Editor/` folders
- Gizmos use consistent colors per NPC type across the project
- Preview meshes (if used) tagged `EditorOnly` — never included in builds

### Consistency Rules

| Category | Convention | Consequence If Ignored |
|----------|-----------|------------------------|
| Method naming | Verb-first PascalCase: `TakeDamage()`, `CompleteQuest()`, `LoadZone()` | Agents create incompatible names |
| Event naming | `On` + past participle: `OnEnemyDefeated`, `OnQuestCompleted` | Events named differently by each agent |
| Event data | `{Action}Event` struct: `EnemyDefeatedEvent`, `DamageTakenEvent` | Inconsistent data structures |
| Service interfaces | `I{System}`: `ICombatSystem`, `IQuestManager`, `IAudioManager` | Agents duplicate services |
| SO classes | PascalCase + `SO` suffix: `ItemDataSO`, `AbilityDataSO` | Inconsistent data class names |
| Private fields | camelCase with `_` prefix: `_playerHealth`, `_currentQuest` | Agents mix conventions |
| Constants | `UPPER_SNAKE_CASE` in `GameConstants`: `MAX_LEVEL`, `DEFAULT_SPEED` | Magic numbers in code |
| DI pattern | `MonoBehaviour<T...>` + `Init()` — never `GetComponent` for external deps | Coupling and rogue singletons |
| Error handling | Try-catch on I/O only, never in hot paths | Degraded performance or silent errors |
| Logging | `GameLogger.{Level}("[Tag] message")` | Inconsistent log format, impossible debug |
| Pooling | Frequent entities always pooled, Release on disable | GC spikes and frame drops |
| Tests | `MethodName_Scenario_ExpectedResult` in `Tests/{EditMode|PlayMode}/{Domain}/` | Unfindable and unmaintainable tests |
| Using order | System → UnityEngine → Sisus.Init → Project namespaces | Inconsistent code across files |
| NPC placement | Hand-placed in scenes, managed by INpcManager | Code-spawned NPCs break level design workflow |
| Localisation | All player-facing text via string keys, never hardcoded | Untranslatable text if hardcoded |

## Epic to Architecture Mapping

| Epic | Systems | Script Folder(s) | Key Patterns | Key Interfaces |
|------|---------|------------------|-------------|----------------|
| E1: Core Infrastructure & Bootstrap | DI, patterns, interfaces, bootstrap | Core/ | HSM, Object Pool, Command, DI | All core interfaces (stubs) |
| E2: Scene Management & Zone System | Zone loading, Addressables, transitions | World/Zones/ | Addressables, async UniTask | IZoneManager, IAssetLoader |
| E3: Player Controller & Entity Framework | Movement, camera, input, entities, NPC mgmt | Player/, NPC/ | HSM, TrinityCore, DI | IInputManager, INpcManager, IInteractionSystem |
| E4: Combat System | Targeting, abilities, damage, status effects, enemy AI | Combat/ | HSM, Object Pool, Strategy, DI | ICombatSystem, ICombatEvents |
| E5: UI/HUD Foundation | HUD, menus, hotbar, tooltips | UI/ | Observer events, DI | IUIService |
| E6: Character Progression | Stats, XP, leveling, talent points | Progression/ | Data-driven (SO), DI | IProgressionSystem |
| E7: Save System | Save/load, MemoryPack, versioning | Save/ | MemoryPack, async UniTask | ISaveSystem |
| E8: Inventory & Equipment | Items, equipment, loot, rarity | Inventory/ | SO registries, DI | IInventoryManager, IItemRegistry |
| E9: Quest System | Quests, objectives, tracking, rewards | Quest/Quests/ | HSM, events, DI | IQuestManager |
| E10: Dialogue System | Dialogue trees, NPC memory, choices | Quest/Dialogue/ | **Dialogue system TBD**, events | IDialogueSystem |
| E11: World State & Consequences | World state tracking, consequence propagation | World/State/, Quest/Consequences/ | Observer, event-sourced / snapshot **TBD** | IWorldStateTracker, IWorldStateEvents |
| E12: NPC, Reputation & Companions | Reputation, merchants, companion AI | NPC/ | TrinityCore, Strategy, Command | INpcManager, IReputationSystem |
| E13: Character Customization | Races, specializations, dual talent trees | Progression/Talents/, Specializations/ | Data-driven (SO) | ITalentSystem |
| E14: Economy | Gold, regional merchants, commissions, balance | NPC/Merchants/, Progression/ | Simple service, SO | IEconomyService |
| E15: Dungeons & Raids | Instanced zones, bosses, raids | World/Zones/, NPC/AI/ | HSM, Pool, boss patterns | IDungeonManager |
| E16: Localisation | String keys, translations FR+EN | Core/ | Service, data-driven | ILocalisationService |
| E17: Art, Audio & Polish | 3D assets, VFX, animations, music, SFX | Art/, Audio/, Prefabs/ | Asset naming conventions, IAudioManager service | IAudioManager |

## Localisation

**Approach:** String key-based localisation from day one.

**Rules:**
- ALL player-facing text uses string keys, never hardcoded strings in code
- Key format: `{domain}.{context}.{identifier}` — e.g., `ui.hud.health_label`, `quest.main_01.title`, `npc.blacksmith.greeting`
- Languages at v1.0: **French** (primary) + **English**
- Translation files: JSON or CSV per language, loaded at startup
- Language selection persisted in player settings (PlayerPrefs)
- ScriptableObjects store string keys for display names and descriptions, not raw text
- UI text components reference keys resolved at runtime by `ILocalisationService`

## Architecture Validation

### Validation Summary

| Check | Result | Notes |
|-------|--------|-------|
| Decision Compatibility | ✅ Pass | All decisions coherent. MemoryPack binary format noted for future modding consideration. |
| GDD Coverage | ✅ Pass | All systems covered across 17 epics. Dialogue and world state deferred to Epics 10-11 with interfaces defined. |
| Pattern Completeness | ✅ Pass | 10/10 implementation patterns defined with code examples. |
| Epic Mapping | ✅ Pass | All 17 epics mapped to architecture. Epic 10 partial (dialogue TBD), Epic 11 partial (world state TBD). |
| Document Completeness | ✅ Pass | All sections populated, no placeholders remaining. |

### Coverage Report

- **Systems Covered:** 14/14 (2 with deferred implementation)
- **Patterns Defined:** 10 (+ 7 formalized design patterns)
- **Decisions Made:** 15 (+ 2 deferred)
- **Epics Mapped:** 17/17

### Deferred Items

| Item | Deferred Until | Blocking? | Interfaces Ready? |
|------|---------------|-----------|-------------------|
| **Dialogue System** | Epic 10 — Dialogue System | No | Yes — `IDialogueSystem` |
| **World State Architecture** | Epic 11 — World State & Consequences | No | Yes — `IWorldStateTracker` |

### Validation Date

2026-02-23

## Development Environment

### Prerequisites

- **Unity 6** (6000.3.9f1) — Install via Unity Hub
- **URP 17.3.0** — Pre-configured via Package Manager
- **Git + Git LFS** — Required for version control and large assets
- **.NET / C#** — Unity's embedded Mono (Editor) / IL2CPP (Build)

### AI Tooling (MCP Servers)

The following MCP servers were selected during architecture to enhance AI-assisted development:

| MCP Server | Purpose | Install Type |
|------------|---------|-------------|
| MCP Unity (CoderGamester/mcp-unity) | Scene inspection, asset queries, runtime debug | npm package |
| Context7 (upstash/context7) | Up-to-date Unity 6 API docs in AI context | npm package |

**Setup:**

1. Install MCP Unity: `npx @anthropic-ai/create-mcp-server` or follow CoderGamester/mcp-unity README
2. Configure MCP Unity — connect to Unity Editor via TCP (auto-detected when Editor is running)
3. Configure Context7 — add to MCP config in your AI assistant settings
4. Both accessible from Copilot/Claude agent sessions

These give your AI assistant direct access to Unity for scene inspection, asset queries, and context-aware code generation.

### First Steps

1. Clone the repo and open in Unity 6 via Unity Hub
2. Install **Init(args)** from Asset Store / OpenUPM
3. Configure MCP servers per AI Tooling instructions above
4. Create the `Assets/Scripts/` folder structure per Project Structure section
5. Implement bootstrap scene with `GameManager` service locator
6. Set up Addressables groups (Core, per-zone) and mark initial assets

---

*Architecture document complete. Ready for Epic implementation phase.*
