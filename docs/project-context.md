# Project Context

Read this file before implementing any feature. It contains the full technology stack, architecture rules, anti-patterns, and project-specific constraints.

---

## Technology Stack

| Layer | Technology | Notes |
|---|---|---|
| Engine | Unity 6 (6000.4.10f1) | |
| Render | URP 17.4.0 | Separate PC / Mobile pipeline assets |
| Input | Unity Input System 1.19.0 | New Input System only |
| Simulation | Unity DOTS (Entities) | DOTS-first for enemies and projectiles |
| Pathfinding | Unity AI Navigation 2.0.13 | NavMesh |
| Cinematics | Unity Timeline 1.8.12 | |
| Testing | Unity Test Framework 1.6.0 | Test Runner only |

No external DI framework. No Addressables (not yet decided). No tween library (not yet decided).

---

## Architecture

### Hybrid ECS — Entity split

| Entity Type | Primary World | Reason |
|---|---|---|
| Common enemies | DOTS-first | High volume |
| Projectiles | DOTS-first | High volume, simple logic |
| Player | MonoBehaviour-first | Unique, complex, input |
| NPCs | MonoBehaviour-first | Rich dialogue, low count |
| Interactables | MonoBehaviour-first | No perf constraint |

NPCs and Player expose shared data (`HealthData`, `FactionData`) to ECS via Baker.

### Scene structure

- **Bootstrap**: persistent, never unloaded. Owns all global services.
- **UI**: persistent additive. HUD, menus, dialogue, loading screen.
- **Zone_[Name]**: additive, swapped. Next zone loads before previous unloads.

### ServiceLocator

Custom static registry. Register in `Awake`, resolve in `Start`. No framework dependency.

### SO Event Channels

Inter-system communication via ScriptableObject channels. Systems never hold direct references to each other. Two types: `VoidEventChannel`, `EventChannel<T>`.

---

## Performance Targets

| Metric | Target | Hard Limit |
|---|---|---|
| Frame time | 16.67ms (60 FPS) | — |
| RAM usage | < 4 GB | < 6 GB |
| Zone load time | < 3s | < 5s |
| Min spec CPU | i3 gen 6/7 | — |
| Min spec GPU | GTX 900 / iGPU | — |

### Hot path rules (Update / FixedUpdate / LateUpdate)

- Never allocate — cache references, use object pooling
- No LINQ — generates garbage via enumerator allocation
- Use `StringBuilder` for string concatenation in loops, not `string +` or `$""`
- Use `CompareTag("tag")` instead of `gameObject.tag == "tag"` (avoids string allocation)
- Object pooling for enemies, projectiles, and VFX
- Profile before optimizing — never prematurely optimize

---

## Anti-Patterns — Never Do These

- **No singletons** (`MyManager.Instance`) — use the ServiceLocator
- **No `FindObjectOfType` / `FindObjectsOfType`** — slow, fragile
- **No `GameObject.Find("name")`** — breaks on rename, slow
- **No `Resources.Load()`** — no `Resources/` folder in this project
- **No legacy input** (`Input.GetKey`, `Input.GetAxis`) — New Input System only
- **No Built-in RP shaders** — URP only, Built-in shaders render pink
- **No `public` fields for Inspector** — use `[SerializeField] private`
- **No `DontDestroyOnLoad` calls** — Bootstrap handles persistence
- **No inheritance chains** (`Entity → Character → Player`) — pure composition
- **No `Update()` polling** when an event suffices

---

## Common Unity Gotchas

- Unity `==` is overloaded: `null` check on a destroyed object returns `true`, but `is null` returns `false`. Always use `== null` for destroyed object checks.
- `OnDestroy` fires on both scene unload and application quit — guard against double-disposal.
- `Awake` / `Start` are not called on disabled GameObjects.
- Coroutines stop when a GameObject is disabled — structure async code accordingly.
- `Time.deltaTime` is 0 in `FixedUpdate` — use `Time.fixedDeltaTime`.
- ScriptableObject instances are shared across all references in builds. Mutations on an SO instance persist until app restart. Never use SO instances as mutable runtime state.
- With Enter Play Mode Options enabled: static fields are NOT reset between plays — always reset statics via `[RuntimeInitializeOnLoadMethod]` or avoid them entirely.
- `OnEnable` / `OnDisable` for event subscription / unsubscription — always match pairs to prevent memory leaks.

---

## URP Rules

- All custom shaders must be URP-compatible (Shader Graph or URP shader API)
- Post-processing via `Volume` components — not legacy PostProcessing Stack
- Camera stacking for layered rendering (world camera + UI camera)

---

## Input Rules

- All input through New Input System action maps
- Reference Input Actions via generated C# class or `PlayerInput` component
- Use action callbacks (`performed`, `canceled`) — not polling in `Update()` unless continuous input is required

---

## Serialization Rules

- `[SerializeField]` for private fields exposed in Inspector
- ScriptableObjects for shared data / configuration — not MonoBehaviours on persistent GameObjects
- Unity cannot serialize: `Dictionary`, interfaces, abstract classes, properties — use serializable wrappers when needed

---

## Valenthia-Specific Constraints

- Zone transitions use additive scene loading — always clean up event subscriptions in `OnDisable` / `OnDestroy`
- World state changes must go through `IWorldStateTracker` — never modify world state directly
- All narrative choices must be tracked via `IWorldStateTracker` for the consequence system
- NPC reputation changes must go through `IReputationSystem`
- Quest state changes only through `IQuestManager` — ensures save system consistency

> `IWorldStateTracker`, `IReputationSystem`, and `IQuestManager` are not yet implemented. These constraints define the intended contracts — implement accordingly when building those systems.
