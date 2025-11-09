# Valenthia Chronicles - AI Coding Agent Instructions

## Core Principles

**ALWAYS write code and comments in English** - No French code, variable names, or comments.

**Keep it simple** - Only implement what the user explicitly requests. Do not add extra features, examples, or "improvements" unless specifically asked.

**Perform web research on main feature** - For feature implementations, search for best practices, examples, and standards in the game industries with Unity.

## Project Overview
Unity 6000.2.10f1 game project using Universal Render Pipeline (URP). Early-stage development with foundational player controls and custom debugging infrastructure.

## Architecture & Code Organization

### Namespace Structure
- **`Sources`**: Core game code (GameManager, utilities)
- **`Sources.Entities.Player`**: Player-related components (Player, InputController, CameraController)
- **`Sources.Editor`**: Custom editor tools (AstralisConsole)

All game scripts use consistent namespace declarations. Never create scripts without proper namespace.

### Component Design Pattern
Components follow a **split responsibility pattern**:
- `Player.cs`: Minimal player entity (position queries only)
- `InputController.cs`: Handles all input via New Input System, movement, and animation
- `CameraController.cs`: Third-person camera with zoom, positioned via `LateUpdate()`

This is intentional separation - keep responsibilities distinct per-component.

## Critical Developer Conventions

### Custom Logging System
**ALWAYS use `AstralisDebug` instead of Unity's `Debug.Log`**:
```csharp
using Sources;

AstralisDebug.Log("Player", "Movement started");
AstralisDebug.LogWarning("Input", "Action not found");
AstralisDebug.LogError("GameManager", "Critical failure");
```

**Predefined categories**: Player, Enemy, UI, Audio, Input, GameManager, AI, Physics, Animation, Save, General

The custom console (`Tools > Astralis Console`) filters by category and severity. Register new categories in `AstralisDebug.cs` if needed.

### Unity New Input System
Input uses the **Input System package** (1.14.2), not legacy Input Manager.
- Actions defined in: `Assets/Settings/InputActionMap.inputactions`
- Access pattern in scripts:
```csharp
inputActionMap = InputSystem.actions;
moveAction = inputActionMap.FindAction("Move");
jumpAction = inputActionMap.FindAction("Jump");
```
Never use `Input.GetKey()` - always use InputAction references.

## Key Systems

### Scene Management
- The game work with additive scene. A core scene (`SC_GameRoot.unity`) manage main systems and a secondary scene (`SC_DevMap.unity`) contains the game design.
- Commented-out manager references suggest planned EventBus/SaveManager architecture

### Third-Person Camera System
`CameraController` uses:
- Fixed angle (45°) with adjustable distance
- Scroll-wheel zoom (5-30 units range)
- Gizmos for editor visualization (yellow arm, green player sphere)
- `FindFirstObjectByType<Player>()` for automatic player reference

## External Dependencies

### TextMesh Pro
Available in `Assets/Plugins/` for UI text rendering.

## Important Notes
- **Player movement**: Rotation = X-axis input, Translation = Y-axis input (tank controls)
- **Component references**: Prefer `GetComponent<>()` in `Awake()`, `FindFirstObjectByType<>()` for singletons
- **Commented code in GameManager**: Indicates planned features (WorldManager, EventBus, SaveManager) - preserve for future implementation

## What's Missing (Do Not Assume)
- No physics-based movement (custom gravity only)
- No pooling systems yet
- No save/load implementation
- No event bus (planned but not active)
- Minimal entity architecture (just Player so far)
