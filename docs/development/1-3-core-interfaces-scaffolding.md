# Story 1.3: Core Interfaces Scaffolding

Status: ready-for-dev

## Story

As a **developer**,
I want **all service interfaces scaffolded as stubs in Scripts/Core/Interfaces/**,
so that **future epics can implement these interfaces without breaking existing code or creating circular dependencies**.

## Acceptance Criteria

1. **All service interfaces created as stubs**
   - `ICombatSystem`, `IQuestManager`, `IWorldStateTracker`, `IZoneManager`
   - `IAudioManager`, `IInventoryManager`, `IProgressionSystem`, `IUIService`
   - `INpcManager`, `IReputationSystem`, `IEconomyService`, `ILocalisationService`
   - `ISaveSystem`, `IDialogueSystem`, `IAssetLoader`, `IInputManager`

2. **Interfaces follow naming conventions**
   - `I{System}` naming pattern
   - Located in `Scripts/Core/Interfaces/`
   - Namespace `ValenthiaChronicles.Core`

3. **Interfaces contain minimal method signatures**
   - Only essential methods that are certain to exist
   - Methods can be empty stubs — implementation comes in later epics
   - Event declarations where applicable

4. **No circular dependencies**
   - Interfaces depend only on Core types (enums, structs, other interfaces)
   - No concrete class references

## USER Manual Steps (Unity Editor)

**⚠️ Ces étapes DOIVENT être faites manuellement par l'utilisateur dans Unity :**

### Step 1: Verify Compilation
1. Retourner dans Unity après l'implémentation LLM
2. Attendre la recompilation automatique
3. Vérifier qu'il n'y a **aucune erreur** dans la Console

---

## LLM Automated Tasks

**✅ Ces tâches seront automatiquement effectuées par le LLM :**

- [ ] **Task 1: Combat & Quest interfaces** (AC: #1)
  - [ ] Create `ICombatSystem.cs`
  - [ ] Create `IQuestManager.cs`
  - [ ] Create `IDialogueSystem.cs`

- [ ] **Task 2: World & Zone interfaces** (AC: #1)
  - [ ] Create `IWorldStateTracker.cs`
  - [ ] Create `IZoneManager.cs`
  - [ ] Create `IAssetLoader.cs`

- [ ] **Task 3: Entity & NPC interfaces** (AC: #1)
  - [ ] Create `INpcManager.cs`
  - [ ] Create `IReputationSystem.cs`

- [ ] **Task 4: Player systems interfaces** (AC: #1)
  - [ ] Create `IInventoryManager.cs`
  - [ ] Create `IProgressionSystem.cs`
  - [ ] Create `IInputManager.cs`

- [ ] **Task 5: Support systems interfaces** (AC: #1)
  - [ ] Create `IAudioManager.cs`
  - [ ] Create `IUIService.cs`
  - [ ] Create `ISaveSystem.cs`
  - [ ] Create `IEconomyService.cs`
  - [ ] Create `ILocalisationService.cs`

## Dev Notes

### Architecture Compliance

Ces interfaces sont des **stubs** — elles définissent le contrat minimal. L'implémentation complète viendra dans les epics correspondantes :

| Interface | Epic d'implémentation |
|-----------|----------------------|
| `ICombatSystem` | E4: Combat System |
| `IQuestManager` | E9: Quest System |
| `IDialogueSystem` | E10: Dialogue System |
| `IWorldStateTracker` | E11: World State |
| `IZoneManager` | E2: Scene Management |
| `IAssetLoader` | E2: Scene Management |
| `INpcManager` | E3: Entity Framework |
| `IReputationSystem` | E12: NPC & Reputation |
| `IInventoryManager` | E8: Inventory |
| `IProgressionSystem` | E6: Character Progression |
| `IInputManager` | E3: Player Controller |
| `IAudioManager` | E17: Audio |
| `IUIService` | E5: UI/HUD |
| `ISaveSystem` | E7: Save System |
| `IEconomyService` | E14: Economy |
| `ILocalisationService` | E16: Localisation |

### References

- [Source: _bmad-output/game-architecture.md#Epic-to-Architecture-Mapping]
- [Source: _bmad-output/plans/epics.md#Story-1.3]
- [Source: docs/init-args-guide.md#Services-globaux-recommandés]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Completion Notes List

### File List

