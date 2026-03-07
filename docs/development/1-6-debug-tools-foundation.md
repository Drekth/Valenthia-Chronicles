# Story 1.6: Debug Tools Foundation

Status: ready-for-dev

## Story

As a **developer**,
I want **debug tools (enhanced GameLogger, debug console, performance overlay) implemented**,
so that **I can efficiently debug and profile the game during development**.

## Acceptance Criteria

1. **GameLogger enhanced with system tags**
   - Support for all system tags: `[Combat]`, `[Quest]`, `[WorldState]`, `[Save]`, `[Audio]`, `[UI]`, `[AI]`, `[Zone]`, `[Input]`, `[Progression]`
   - Log levels: ERROR, WARN, INFO, DEBUG
   - DEBUG level stripped from release builds via `[Conditional]`

2. **Debug console functional**
   - Activated via tilde (`~`) key
   - Command registration system
   - Basic commands: `/help`, `/god`, `/setlevel`, `/give`, `/teleport`
   - Editor-only (`#if UNITY_EDITOR || DEVELOPMENT_BUILD`)

3. **Performance overlay functional**
   - Activated via `F3` key
   - Shows: FPS, frame time, memory usage
   - Optional in release builds (settings toggle)

## LLM Automated Tasks

- [ ] **Task 1: Enhance GameLogger with tags and levels**
- [ ] **Task 2: Create DebugConsole MonoBehaviour**
- [ ] **Task 3: Create PerformanceOverlay MonoBehaviour**
- [ ] **Task 4: Create debug command system**

## Dev Notes

### Architecture

```
Scripts/Core/Debug/
├── DebugConsole.cs
├── PerformanceOverlay.cs
└── Commands/
    └── IDebugCommand.cs
```

### References

- [Source: _bmad-output/game-architecture.md#Debug-Development-Tools]
- [Source: _bmad-output/game-architecture.md#Logging]

