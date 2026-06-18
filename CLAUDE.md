# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

> **Before implementing any feature**, read [docs/project-context.md](docs/project-context.md) — it contains the full architecture rules, anti-patterns, asset naming, and project-specific constraints.

## Project

**Valenthia Chronicles** — top-down heroic fantasy RPG, mid-poly, inspired by WoW and The Elder Scrolls. Primary focus: **lore** and **performance**.

Engine: Unity 6 (`6000.4.10f1`) — open via Unity Hub.

---

## Engine & Tooling

- **Render Pipeline**: URP 17.4.0. Single PC pipeline ([Assets/Settings/PC_RPAsset.asset](Assets/Settings/PC_RPAsset.asset) + [PC_Renderer.asset](Assets/Settings/PC_Renderer.asset), Deferred). Only one quality level (`PC`); the Mobile pipeline/quality level was removed.
- **Input**: Unity Input System 1.19.0. Action map: [Assets/InputSystem_Actions.inputactions](Assets/InputSystem_Actions.inputactions).
- **Unity MCP**: local server at `http://localhost:8080/mcp` (CoplayDev/unity-mcp). Must be started from within the Unity Editor.

## Key Packages

| Package | Version | Purpose |
|---|---|---|
| `com.unity.render-pipelines.universal` | 17.4.0 | URP |
| `com.unity.inputsystem` | 1.19.0 | New Input System |
| `com.unity.ai.navigation` | 2.0.13 | NavMesh / pathfinding |
| `com.unity.timeline` | 1.8.12 | Cinematics / sequencing |
| `com.unity.visualscripting` | 1.9.11 | Visual Scripting |
| `com.unity.test-framework` | 1.6.0 | Unity Test Runner |

**Dependency rule**: before adding any third-party package, check if Unity 6 covers the need natively. Minimize external dependencies to ease Unity version upgrades.

---

## Architecture

### Scene Structure — Single Entry Point + Additive

```
Bootstrap (persistent, never unloaded)
├── ServiceLocator
├── AudioManager
├── SaveSystem
├── TimeManager
└── EventBus (SO Channels)

UI (persistent additive)
└── HUD, Menus, Dialogue, Loading screen

Zone_[Name] (additive, swapped)
├── Terrain + Geometry
├── Entities
└── Lighting / Volume
```

Bootstrap is the only entry point. It loads the UI scene and the first zone. Zones swap: next is loaded before the previous is unloaded (smooth transitions, no pop).

---

### ServiceLocator

Lightweight custom static registry, no external dependency.

```csharp
// Registration (in service Awake)
ServiceLocator.Register<AudioManager>(this);

// Resolution (in consumer Start)
ServiceLocator.Get<AudioManager>().Play(...);
```

Order guaranteed by Unity Script Execution Order (Bootstrap runs first).

---

### SO Event Channels

Systems do not reference each other directly. They communicate via shared ScriptableObjects:

```
OnPlayerDeath  (VoidEventChannel SO)
    ← AudioManager   → plays death sound
    ← UIManager      → shows death screen
    ← SaveSystem     → triggers autosave
    ← QuestManager   → checks fail conditions
```

Two channel types:
- `VoidEventChannel` — payload-less notification
- `EventChannel<T>` — typed payload (int, float, string, EntityRef…)

---

### Core Systems Status

| System | Complexity | Status |
|---|---|---|
| `SceneLoader` | Low | To implement |
| `AudioManager` | Medium | To implement |
| `TimeManager` | Low | To implement |
| `SaveSystem` | High | Architecture pending |
| `QuestManager` | High | Architecture pending (lore-critical) |
| `DialogueSystem` | High | Architecture pending (lore-critical) |

---

## File Structure

```
Assets/
  Arts/        — Art assets (textures, models, audio…)
  Plugins/     — Third-party plugins
  Prefabs/     — Reusable prefabs
  Scenes/      — Unity scenes (Bootstrap, UI, zones…)
  Scripts/     — All C# code
  Settings/    — URP pipeline and renderer assets
  Developper/  — Local dev tools, git-ignored
docs/
  project-context.md  — Full implementation reference (read before coding)
```

---

## Coding Conventions

C++ style (close to Unreal Engine) — explicit, no unnecessary syntax sugar. Enforced via [.editorconfig](.editorconfig).

**All code, comments, commit messages, and documentation must be written in English.**

### Naming — PascalCase everywhere

```csharp
private int MaxHealth;
public float MoveSpeed;
private void TakeDamage() { }
int LocalCounter = 0;
```

Interfaces are prefixed `I` (`IDamageable`, `IInitializable`). No other prefix conventions.

### Condition style

Don't write if/else/for condition on a single line. Do this :

```csharp
if(...)
  do_something()
```

### Asset naming — Unreal Engine style prefixes

| Asset Type | Prefix | Example |
|---|---|---|
| Scene | `SC_` | `SC_Village_001.unity` |
| Prefab | `PF_` | `PF_Player.prefab` |
| Texture | `TX_` | `TX_Player_Armor_Diffuse.png` |
| Material | `MT_` | `MT_Player_Skin.mat` |
| Static Mesh | `SM_` | `SM_Tree_Oak.fbx` |
| Animation | `AN_` | `AN_Player_Idle.anim` |
| Music | `MU_` | `MU_Combat_Boss.ogg` |
| Sound Effect | `SFX_` | `SFX_Sword_Hit_01.wav` |
| VFX | `VFX_` | `VFX_Fireball.prefab` |
| UI Sprite | `UI_` | `UI_Button_Play.png` |
| Data ScriptableObject | `DT_` | `DT_CreatureRegistry.asset` |
| Config ScriptableObject | `CFG_` | `CFG_GameBalance.asset` |

### Member ordering inside a class

```
1. Constants
2. Public properties
3. Methods  (public → protected → private)
4. Fields   (always last)
```

### Section comments

```csharp
//////////////////////////////////////////////////////
/// Section Title                                  ///
//////////////////////////////////////////////////////
```

### File organization

- Minimize file count — multiple related structs, enums, or small classes may live in the same file
- Do not create a file for a single small type
- Shared definitions (enums, lightweight structs, constants, delegates) go in `XxxDefines.cs` files:

```
Entities/CombatDefines.cs   — DamageType, AttackFlags, CombatResult
Entities/StatsDefines.cs    — StatType, StatModifier
Core/GameDefines.cs         — GameState, FactionType
```

### General rules

- No namespaces — all types live in the global namespace
- `private` always written explicitly, never implicit
- No `var` — explicit types (except obvious `foreach`)
- Braces required on all blocks, even single-line
- No `#region`
- No polling in `Update()` when an event suffices
- No magic numbers — named constants or ScriptableObject config
- Keep code simple — avoid complex object structures, favor readability
- Unity attributes accepted (`[SerializeField]`, `[Header]`, etc.) — avoid custom attributes unless clearly justified

---

## Notes

- `.meta` files: always commit alongside their asset — Unity uses them for GUIDs.
- `Assets/Developper/`: git-ignored, for local scratch work and editor tooling.
- Tests via **Unity Test Runner** (Window → General → Test Runner).
