# Project Context for AI Agents

_This file contains critical rules and patterns that AI agents must follow when implementing game code in this project. Focus on unobvious details that agents might otherwise miss._

## Critical Implementation Rules

### Engine-Specific Rules (Unity 6)

#### Init(args) — Dependency Injection (see docs/init-args-guide.md for full reference)
- **ALL component dependencies** must be received via `MonoBehaviour<T...>` + `Init()` — NEVER use singletons, `FindObjectOfType`, or `GetComponent` for external dependencies
- Use `[Service]` attribute for global systems (GameManager, QuestManager, CombatSystem, etc.)
- Use **Service Tags** for scene-scoped services (ZoneController, EnemySpawner, DungeonManager)
- Always prefer **interfaces** as service types (`IInputManager`, `ICombatSystem`, `IQuestManager`) for testability and flexibility
- Override `OnAwake()` NOT `Awake()`, and `OnReset()` NOT `Reset()` — MonoBehaviour<T> owns those methods
- Use `using Sisus.Init;` namespace in all files using Init(args)
- Global services are initialized **before scene load** in dependency order — no execution order worries
- For cross-scene references (zone system): use `Any<T>` fields or Initializers with cross-scene drag & drop
- For plain C# data classes on GameObjects: use `Wrapper<T>` pattern
- For code instantiation: use `gameObject.AddComponent<TComponent, T...>(args)` or `prefab.Instantiate(args)`
- Null Argument Guard catches missing dependencies in Editor and at Runtime — keep it enabled

#### Lifecycle & Initialization
- `Init()` runs before `Awake`/`OnEnable` — dependencies are always available in lifecycle methods
- With Enter Play Mode Options enabled: static fields are NOT reset — always reset statics in `[RuntimeInitializeOnLoadMethod]` or avoid them entirely
- `OnEnable`/`OnDisable` for event subscription/unsubscription — always match pairs to prevent memory leaks

#### Async Patterns
- Use `UniTask` instead of coroutines for all async operations
- Use `async UniTaskVoid` for fire-and-forget (replaces `StartCoroutine`)
- Use `cancellationToken = this.GetCancellationTokenOnDestroy()` to auto-cancel on GameObject destruction
- NEVER use `Task.Run()` — Unity API is NOT thread-safe, stay on main thread unless doing pure computation
- Until UniTask is installed: coroutines are acceptable, but structure code for easy migration

#### Serialization
- Use `[SerializeField]` for private fields exposed in Inspector — NEVER make fields `public` just for Inspector access
- Use `[field: SerializeField]` for auto-properties when needed
- ScriptableObjects for shared data/configuration — NOT MonoBehaviours on persistent GameObjects
- Unity cannot serialize: `Dictionary`, interfaces, abstract classes, properties — use serializable wrappers or custom serialization

#### Input System
- All input must go through the New Input System action maps — NEVER use `Input.GetKey()`, `Input.GetAxis()`, etc.
- Reference Input Actions via generated C# class or `PlayerInput` component
- Use action callbacks (`performed`, `canceled`) — NOT polling in `Update()` unless needed for continuous input

#### URP Rendering
- All custom shaders must be URP-compatible (Shader Graph or URP shader API)
- Do NOT use Built-in RP shaders — they will render pink/broken
- Use `Volume` components for post-processing — NOT legacy `PostProcessingStack`
- Camera stacking and render features for advanced rendering needs

### Performance Rules

- **Frame Budget:** 16.67ms per frame (60 FPS target) — profile regularly with Unity Profiler
- **Memory Target:** < 6 GB on minimum specs (i3 gen 6/7, 8 GB RAM, GTX 900/iGPU)
- **Zone Load Time:** < 5s max, < 3s target per scene transition
- When PrimeTween is installed: use it for ALL tweens/animations — zero-allocation by design
- NEVER allocate in Update/FixedUpdate/LateUpdate hot paths — cache references, use object pooling
- Avoid LINQ in runtime hot paths — generates garbage via enumerator allocation
- Use `StringBuilder` for string concatenation in loops — NOT `string +` or `$""`
- Prefer `CompareTag("tag")` over `gameObject.tag == "tag"` (avoids string allocation)
- Use object pooling for frequently spawned/destroyed objects (enemies, projectiles, VFX)
- Profile before optimizing — do NOT prematurely optimize, measure first
- Zone-based scene loading: unload previous zone assets to prevent memory leaks
- Addressables for asset loading when implemented — lazy load zone assets on demand

### Code Organization Rules

#### Folder Structure (Assets/)
- `Assets/Scripts/` — All gameplay code
  - `Core/` — Global services, base classes, interfaces, utilities
  - `Player/` — Player controller, input handling, camera
  - `Combat/` — Combat system, abilities, status effects, damage
  - `Quest/` — Quest system, dialogue, narrative choices
  - `World/` — Zone management, world state, scene loading
  - `NPC/` — NPC behavior, reputation, merchants, companions
  - `Inventory/` — Items, equipment, inventory management
  - `Progression/` — XP, leveling, talents, specializations
  - `UI/` — All UI scripts (HUD, menus, dialogue UI)
  - `Audio/` — Audio management scripts
  - `Save/` — Save/load system
- `Assets/Arts/` — 3D models, textures, materials, animations
- `Assets/Prefabs/` — All prefabs organized by system
- `Assets/Data/` — Data assets (items, quests, abilities, etc.)
- `Assets/Scenes/` — All scenes (zones, dungeons, towns, menus)
- `Assets/Settings/` — URP assets, Volume profiles (already present)
- **NO `Assets/Resources/` folder** — use Addressables instead

#### Asset Naming — Unreal Engine Style Prefixes

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
| Data (ScriptableObject) | `DT_` | `DT_CreatureRegistry.asset` |
| Config (ScriptableObject) | `CFG_` | `CFG_GameBalance.asset` |

#### Script & Code Naming Conventions
- **Scripts/Classes:** PascalCase, descriptive nouns — `QuestManager`, `PlayerController`, `EnemySpawner`
- **Interfaces:** PascalCase with `I` prefix — `IInputManager`, `ICombatSystem`, `IQuestManager`
- **MonoBehaviours:** PascalCase, descriptive — one primary MonoBehaviour per file recommended, but related types (enums, structs, small helper classes) may live in the same file
- **ScriptableObject classes:** PascalCase with `Template` suffix — `ItemDataTemplate`, `AbilityDataTemplate`
- **Private fields:** camelCase — `playerHealth`, `currentQuest`
- **Public properties:** PascalCase — `MaxHealth`, `CurrentLevel`
- **Methods:** PascalCase, verb-first — `TakeDamage()`, `CompleteQuest()`, `LoadZone()`
- **Constants:** PascalCase — `MaxLevel`, `DefaultSpeed`

#### Code Style
- Multiple related types (enums, structs, small classes) may live in the same file — minimize file count
- Do NOT use namespaces — all types live in the global namespace
- Use Assembly Definitions to organize code into compilation units
- All code, comments, and documentation must be written in English

### Critical Don't-Miss Rules

#### Anti-Patterns — NEVER Do These
- NEVER use `Singleton.Instance` pattern — use Init(args) `[Service]` instead
- NEVER use `FindObjectOfType` / `FindObjectsOfType` — use Init(args) injection or `Find` API
- NEVER use `GameObject.Find("name")` — fragile, slow, breaks on rename
- NEVER use `Resources.Load()` — no Resources folder in this project
- NEVER use legacy `Input.GetKey/GetAxis` — New Input System only
- NEVER use Built-in RP shaders — URP only
- NEVER put game logic in `Update()` when event-driven is possible
- NEVER use `public` fields for Inspector — use `[SerializeField] private`
- NEVER use `DontDestroyOnLoad` manually — Init(args) global services handle persistence

#### Common Gotchas
- Unity `==` operator is overloaded: `null` check on destroyed objects returns `true` but `is null` returns `false` — use Unity's `== null` for destroyed objects
- `OnDestroy` is called on scene unload AND on application quit — guard against double-disposal
- `Awake`/`Start` are NOT called on disabled GameObjects — Init(args) async init handles this
- Coroutines stop when a GameObject is disabled — UniTask does NOT (advantage)
- `Time.deltaTime` is 0 in `FixedUpdate` — use `Time.fixedDeltaTime` instead
- ScriptableObject instances in builds are shared — mutations persist until app restart, never between sessions

#### Valenthia-Specific Rules
- Zone transitions use scene loading — always clean up subscriptions in `OnDisable`/`OnDestroy`
- World state changes must go through `IWorldStateTracker` — NEVER modify world state directly
- All narrative choices must be tracked via `IWorldStateTracker` for consequence system
- NPC memory/reputation changes must go through `IReputationSystem`
- Quest state changes only through `IQuestManager` — ensures save system consistency

---

## Usage Guidelines

**For AI Agents:**
- Read this file before implementing any game code
- Follow ALL rules exactly as documented
- When in doubt, prefer the more restrictive option
- See `docs/init-args-guide.md` for full Init(args) reference and patterns
- Update this file if new patterns emerge
