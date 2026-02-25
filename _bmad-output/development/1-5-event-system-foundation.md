# Story 1.5: Event System Foundation

Status: ready-for-dev

## Story

As a **developer**,
I want **event data structs and event interfaces implemented in Scripts/Core/Events/**,
so that **systems can communicate via the Observer pattern through DI-injected interfaces without tight coupling**.

## Acceptance Criteria

1. **Event data structs created**
   - `EnemyDefeatedEvent`, `DamageTakenEvent`, `DamageDealtEvent`
   - `QuestStartedEvent`, `QuestCompletedEvent`, `QuestFailedEvent`, `ObjectiveUpdatedEvent`
   - `WorldStateChangedEvent`, `ConsequenceTriggeredEvent`
   - `ZoneLoadedEvent`, `ZoneUnloadedEvent`
   - All structs are `readonly` with public readonly fields

2. **Event interfaces created**
   - `ICombatEvents` — combat-related events
   - `IQuestEvents` — quest state events
   - `IWorldStateEvents` — world state mutation events
   - `IZoneEvents` — zone loading events

3. **Naming conventions followed**
   - Event structs: `{Action}Event` suffix
   - Event interface methods: `On` + past participle
   - Events carry data objects, never raw parameters

4. **No circular dependencies**
   - Events depend only on Core types

## LLM Automated Tasks

- [ ] **Task 1: Create combat event structs**
- [ ] **Task 2: Create quest event structs**
- [ ] **Task 3: Create world state event structs**
- [ ] **Task 4: Create zone event structs**
- [ ] **Task 5: Create ICombatEvents interface**
- [ ] **Task 6: Create IQuestEvents interface**
- [ ] **Task 7: Create IWorldStateEvents interface**
- [ ] **Task 8: Create IZoneEvents interface**

## Dev Notes

### Architecture Pattern

```csharp
// Event data struct — readonly, immutable
public readonly struct EnemyDefeatedEvent
{
    public readonly int EnemyId;
    public readonly Vector3 Position;
    public readonly int XpReward;
}

// Event interface — injected via Init(args)
public interface ICombatEvents
{
    event Action<EnemyDefeatedEvent> OnEnemyDefeated;
    event Action<DamageTakenEvent> OnDamageTaken;
}

// Listener pattern
void OnEnable() => _combatEvents.OnEnemyDefeated += HandleEnemyDefeated;
void OnDisable() => _combatEvents.OnEnemyDefeated -= HandleEnemyDefeated;
```

### References

- [Source: _bmad-output/game-architecture.md#Event-System]
- [Source: _bmad-output/game-architecture.md#Observer-Event-System]

