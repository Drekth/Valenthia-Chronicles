# Valenthia Chronicles — Coding Rules

Valenthia Chronicles is a narrative-driven RPG built with Unity 6 (URP), using Init(args) for dependency injection, the New Input System, and modern C# patterns. The architecture follows a modular entity model adapted for Unity. You can find the full description in `docs/game-brief.md`.

## Technology Stack & Versions

- **Engine:** Unity 6 (6000.3.10f1)
- **Render Pipeline:** Universal Render Pipeline (URP) 17.3.0
- **Input:** New Input System 1.18.0 — DO NOT use legacy UnityEngine.Input
- **Navigation:** AI Navigation 2.0.11 or later
- **Testing:** Unity Test Framework 1.6.0 or later
- **C#:** Latest Unity 6 supported version
- **Plugins:** Hot Reload, vInspector, vHierarchy
- **Planned:** Init Args (DI), PrimeTween (animation), UniTask (async/await zero-alloc)
- **Color Style:** .editorconfig (configured in project settings)

## Development Rules

1. **Always write in English** — all code, comments, commit messages, and documentation must be in English.

2. **No namespaces** — do not wrap classes in namespaces. Keep all types in the global namespace.

3. **Minimize file count** — multiple classes, structs, or enums may live in the same file when they are closely related. Do not create a new file for every small type.

4. **Member ordering inside a class** — place members in this order:
   1. Properties (public, then private)
   2. Methods (public, then private)
   3. Fields (serialized, then private)

5. **Respect the `.editorconfig`** — all formatting (indentation, braces, spacing, naming conventions) is defined in the project's `.editorconfig`. Follow it strictly.

6. **Read `docs/project-context.md`** before implementing any feature — it contains the full technology stack, architecture rules, anti-patterns, naming conventions, and project-specific constraints.

7. Commentary section are defines as following:
   //////////////////////////////////////////////////////
   ///               Core Unit Class                  ///
   //////////////////////////////////////////////////////

   #region Unit Core
        
   #endregion