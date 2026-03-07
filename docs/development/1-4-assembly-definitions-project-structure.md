# Story 1.4: Assembly Definitions & Project Structure

Status: ready-for-dev

## Story

As a **developer**,
I want **the complete Scripts/ folder structure with Assembly Definitions per domain**,
so that **compilation is isolated per domain, dependencies are explicit, and build times are optimized**.

## Acceptance Criteria

1. **Complete folder structure created**
   - Core/, Player/, Combat/, Quest/, World/, NPC/, Inventory/, Progression/, UI/, Audio/, Save/
   - Each domain has appropriate subfolders per architecture

2. **Assembly Definitions per domain**
   - `ValenthiaChronicles.Core.asmdef` — foundation, no dependencies
   - `ValenthiaChronicles.Player.asmdef` — depends on Core
   - `ValenthiaChronicles.Combat.asmdef` — depends on Core
   - `ValenthiaChronicles.Quest.asmdef` — depends on Core
   - `ValenthiaChronicles.World.asmdef` — depends on Core
   - `ValenthiaChronicles.NPC.asmdef` — depends on Core
   - `ValenthiaChronicles.Inventory.asmdef` — depends on Core
   - `ValenthiaChronicles.Progression.asmdef` — depends on Core
   - `ValenthiaChronicles.UI.asmdef` — depends on Core
   - `ValenthiaChronicles.Audio.asmdef` — depends on Core
   - `ValenthiaChronicles.Save.asmdef` — depends on Core

3. **Test Assembly Definitions**
   - `ValenthiaChronicles.Tests.EditMode.asmdef` — references all domain asmdefs
   - `ValenthiaChronicles.Tests.PlayMode.asmdef` — references all domain asmdefs

4. **No circular dependencies**
   - Domain assemblies depend ONLY on Core
   - Cross-domain communication via Core interfaces

## USER Manual Steps (Unity Editor)

**⚠️ Aucune étape manuelle requise — tout est automatisé.**

### Step 1: Verify Compilation
1. Retourner dans Unity après l'implémentation LLM
2. Attendre la recompilation automatique
3. Vérifier qu'il n'y a **aucune erreur** dans la Console
4. Vérifier que les tests passent toujours dans Test Runner

---

## LLM Automated Tasks

**✅ Ces tâches seront automatiquement effectuées par le LLM :**

- [ ] **Task 1: Create domain folder structure**
- [ ] **Task 2: Create Core.asmdef**
- [ ] **Task 3: Create domain asmdefs (Player, Combat, Quest, World, NPC, Inventory, Progression, UI, Audio, Save)**
- [ ] **Task 4: Create/Update Tests asmdefs**

## Dev Notes

### Assembly Definition Dependencies

```
Core (no deps)
  ↑
  ├── Player
  ├── Combat
  ├── Quest
  ├── World
  ├── NPC
  ├── Inventory
  ├── Progression
  ├── UI
  ├── Audio
  └── Save

Tests.EditMode → All domains
Tests.PlayMode → All domains
```

### References

- [Source: _bmad-output/game-architecture.md#Project-Structure]
- [Source: _bmad-output/game-architecture.md#Architectural-Boundaries]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### File List

