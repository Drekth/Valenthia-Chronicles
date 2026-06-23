# Narrative System Architecture — Quests & Dialogue

Reference document for the narrative stack: **WorldState**, **Quests**, **Reputation**, and the
**node-graph Dialogue system** (runtime + custom editor). Read alongside
[project-context.md](project-context.md) — every rule there (no namespaces, no singletons, pure
composition, `[SerializeField] private`, EventBus, ServiceLocator, hot-path discipline) applies
here and is not repeated.

> Status: **design approved**. Quest data model and the node-graph dialogue editor (option B,
> fully native — no external dependency) are the agreed direction. This document is the
> implementation contract.

---

## 1. Scope & guiding principles

The narrative stack has four layers. Each depends only on the one below it; none reference each
other directly — all cross-layer communication goes through the **EventBus** or the
**ServiceLocator**.

```
EventBus (kill / pickup / zone-enter / dialogue-choice …)
        │
        ▼
WorldState  ──►  SaveSystem        (IWorldStateTracker — single source of truth, persisted)
        │
        ├──►  QuestManager          (IQuestManager — stages + objectives)
        ├──►  ReputationSystem      (IReputationSystem — faction standing)
        │
        ▼
Dialogue (DialogueRunner)           (reads conditions, writes effects, drives UI via EventBus)
```

Principles:

1. **WorldState is the single source of truth.** Quests, reputation and dialogue never hold
   authoritative narrative state of their own — they read and write flags/variables in the
   WorldState. This is what makes the SaveSystem trivial and the consequence system possible.
2. **Definitions are immutable ScriptableObjects; state is runtime-only.** `QuestData` and
   `DialogueData` are `DT_` assets, shared and never mutated at runtime (see the SO mutation
   gotcha in project-context). Progress and "seen" state live in runtime instances and in the
   WorldState.
3. **Conditions and effects are one shared, polymorphic library.** The same
   `INarrativeCondition` / `INarrativeEffect` types (`[SerializeReference]`, the `SpellEffect`
   pattern) gate quest stages, quest availability, and dialogue branches/actions. Adding a new
   predicate or consequence is one new class, used everywhere.
4. **Editor tooling is layered on top of a data model that stands alone.** The dialogue graph
   is authorable as a plain asset before the visual editor exists; the GraphView window is a
   convenience over that data, never a hard requirement for it.

---

## 2. Folder & assembly layout

```
Assets/Scripts/Narrative/
  NarrativeDefines.cs          — shared enums: FactionId, RelationLevel, WorldValueType …
  NarrativeEvents.cs           — IEvent structs for the whole narrative stack
  Conditions/
    INarrativeCondition.cs     — abstract base (SerializeReference)
    FlagCondition.cs, ReputationCondition.cs, QuestStateCondition.cs, HasItemCondition.cs
  Effects/
    INarrativeEffect.cs        — abstract base (SerializeReference)
    SetFlagEffect.cs, ModifyReputationEffect.cs, GiveQuestEffect.cs, AdvanceQuestEffect.cs,
    GiveItemEffect.cs
  WorldState/
    IWorldStateTracker.cs, WorldStateTracker.cs, WorldStateSnapshot.cs
  Reputation/
    IReputationSystem.cs, ReputationSystem.cs
  Quests/
    IQuestManager.cs, QuestManager.cs
    QuestData.cs               — DT_ ScriptableObject (definition)
    QuestStage.cs, QuestObjective.cs (+ Kill/Collect/Reach/Talk/Flag objectives)
    ActiveQuest.cs             — runtime instance (progress)
  Dialogue/
    DialogueData.cs            — DT_ ScriptableObject (graph asset)
    DialogueNode.cs            — abstract base + node types (SerializeReference)
    DialogueRunner.cs          — runtime traversal service
    DialogueDefines.cs
    Editor/                    — separate Editor assembly (see below)
      DialogueGraphEditorWindow.cs
      DialogueGraphView.cs
      DialogueNodeView.cs
      DialogueSearchProvider.cs
      DialogueGraphSerializer.cs
```

- All runtime code: global namespace, `Assembly-CSharp` (matching the rest of the project).
- **Editor code lives under `Dialogue/Editor/` behind its own `.asmdef`** that references the
  runtime assembly and `UnityEditor`. GraphView lives in
  `UnityEditor.Experimental.GraphView` (Editor-only) — it must never be referenced by runtime
  code, or builds break.

Assets (authored data) follow the asset-naming table:

```
Assets/Arts/Data/Quests/      DT_Quest_MissingCaravan.asset
Assets/Arts/Data/Dialogues/   DT_Dialogue_GuardElric.asset
```

---

## 3. Shared narrative primitives

### 3.1 Conditions

```csharp
//////////////////////////////////////////////////////
/// Narrative condition — polymorphic predicate     ///
//////////////////////////////////////////////////////

// A read-only test against the world. Used to gate quest availability, quest stages and
// dialogue branches. [SerializeReference] so the Inspector managed-reference picker adds
// concrete conditions without a custom drawer — same pattern as SpellEffect.
[Serializable]
public abstract class INarrativeCondition
{
    public abstract bool Evaluate(in NarrativeContext Context);
}

[Serializable]
public class FlagCondition : INarrativeCondition
{
    public override bool Evaluate(in NarrativeContext Context)
    {
        return Context.World.GetBool(Key) == Expected;
    }

    [SerializeField] private string Key;
    [SerializeField] private bool   Expected = true;
}

[Serializable]
public class ReputationCondition : INarrativeCondition
{
    public override bool Evaluate(in NarrativeContext Context)
    {
        return Context.Reputation.GetRelation(Faction) >= MinimumRelation;
    }

    [SerializeField] private FactionId     Faction;
    [SerializeField] private RelationLevel MinimumRelation = RelationLevel.Friendly;
}
```

`NarrativeContext` is a lightweight readonly struct passed by `in` (no allocation) carrying the
resolved service references:

```csharp
public readonly struct NarrativeContext
{
    public readonly IWorldStateTracker World;
    public readonly IReputationSystem  Reputation;
    public readonly IQuestManager      Quests;
    public readonly Unit               Player;   // the interacting actor

    public NarrativeContext(IWorldStateTracker World, IReputationSystem Reputation,
                            IQuestManager Quests, Unit Player)
    {
        this.World      = World;
        this.Reputation = Reputation;
        this.Quests     = Quests;
        this.Player     = Player;
    }
}
```

### 3.2 Effects

```csharp
// A mutation of the world. Used by quest-stage completion and dialogue action nodes. All
// state writes funnel through here, so the consequence/save systems see every change.
[Serializable]
public abstract class INarrativeEffect
{
    public abstract void Apply(in NarrativeContext Context);
}

[Serializable]
public class SetFlagEffect : INarrativeEffect
{
    public override void Apply(in NarrativeContext Context)
    {
        Context.World.SetBool(Key, Value);
    }

    [SerializeField] private string Key;
    [SerializeField] private bool   Value = true;
}

[Serializable]
public class GiveQuestEffect : INarrativeEffect
{
    public override void Apply(in NarrativeContext Context)
    {
        Context.Quests.StartQuest(Quest);
    }

    [SerializeField] private QuestData Quest;
}
```

> Naming note: `INarrativeCondition` / `INarrativeEffect` are abstract classes, not C#
> interfaces — Unity cannot serialize interface references. The `I` prefix is kept for
> readability per the project convention; they behave like the abstract `SpellEffect` base.

---

## 4. WorldState

The authoritative key→value store. Keys are strings (designer-authored, stable); values are
bool / int / float. No other system stores narrative truth.

```csharp
public interface IWorldStateTracker
{
    bool  GetBool(string Key);
    int   GetInt(string Key);
    float GetFloat(string Key);

    void  SetBool(string Key, bool Value);
    void  SetInt(string Key, int Value);
    void  SetFloat(string Key, float Value);

    // Raised after any write so listeners (UI, reactive NPCs) react without polling.
    WorldStateSnapshot CaptureSnapshot();
    void               RestoreSnapshot(WorldStateSnapshot Snapshot);
}
```

- Registered in the Bootstrap scene: `ServiceLocator.Register<IWorldStateTracker>(this)` in
  `Awake`.
- Every `Set*` raises a `WorldStateChangedEvent { string Key }` on the EventBus. Reactive
  systems subscribe instead of polling in `Update()`.
- `CaptureSnapshot` / `RestoreSnapshot` are the only contact points with the SaveSystem — the
  snapshot is a serializable struct of three `List<KeyValue>` (Unity cannot serialize
  `Dictionary`; the runtime store uses dictionaries, the snapshot uses lists).

---

## 5. Reputation

Thin layer over WorldState for faction standing. Kept separate because dialogue conditions read
it constantly and it has its own discrete `RelationLevel` semantics.

```csharp
public interface IReputationSystem
{
    int           GetValue(FactionId Faction);
    RelationLevel GetRelation(FactionId Faction);   // derived from value thresholds
    void          Modify(FactionId Faction, int Delta);
}
```

- `RelationLevel` (in `NarrativeDefines.cs`): `Hostile, Unfriendly, Neutral, Friendly, Honored,
  Exalted` — WoW-style bands derived from the raw int.
- Backed by WorldState (`SetInt($"rep.{Faction}", …)`) so reputation is saved for free.
- `Modify` raises `ReputationChangedEvent { FactionId Faction, int NewValue }`.

---

## 6. Quest system

> This is the approved model from the design discussion. Definition = immutable SO; progress =
> runtime instance; truth = WorldState.

### 6.1 Definition (ScriptableObject)

```csharp
[CreateAssetMenu(menuName = "Valenthia/Narrative/Quest")]
public class QuestData : ScriptableObject
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public string                    Id     => QuestId;
    public string                    Title  => QuestTitle;
    public IReadOnlyList<QuestStage> Stages => QuestStages;

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [Header("Identity")]
    [SerializeField] private string QuestId;
    [SerializeField] private string QuestTitle;
    [TextArea]
    [SerializeField] private string QuestSummary;

    [Header("Availability")]
    [SerializeReference] private List<INarrativeCondition> Prerequisites = new();

    [Header("Flow")]
    [SerializeField] private List<QuestStage> QuestStages = new();
}

[Serializable]
public class QuestStage
{
    public string                       Description => StageDescription;
    public IReadOnlyList<QuestObjective> Objectives => StageObjectives;
    public IReadOnlyList<INarrativeEffect> OnComplete => CompletionEffects;

    [SerializeField]     private string                  StageDescription;   // journal text
    [SerializeReference] private List<QuestObjective>    StageObjectives  = new();
    [SerializeReference] private List<INarrativeEffect>  CompletionEffects = new();
}
```

### 6.2 Objectives (polymorphic)

```csharp
// What must happen, not how far we are. Progress lives on ActiveQuest, never on the SO.
[Serializable]
public abstract class QuestObjective
{
    public abstract string Describe();                       // "Defeat wolves"
    public abstract int    RequiredAmount { get; }           // 3
}

[Serializable]
public class KillObjective : QuestObjective
{
    public override string Describe()      => $"Defeat {TargetId}";
    public override int    RequiredAmount  => RequiredCount;

    [SerializeField] private string TargetId;
    [SerializeField] private int    RequiredCount = 1;
}
```

Concrete v1 set: `KillObjective`, `CollectObjective`, `ReachObjective` (zone), `TalkObjective`
(dialogue node reached), `FlagObjective` (world flag set). Each advances by listening to an
EventBus event — see §6.4.

### 6.3 Runtime instance & manager

```csharp
public interface IQuestManager
{
    void          StartQuest(QuestData Quest);
    void          AdvanceStage(string QuestId);
    QuestProgress GetProgress(string QuestId);   // None / Active / Completed + stage index
    bool          IsActive(string QuestId);

    // Save contact point.
    QuestJournalSnapshot CaptureSnapshot();
    void                 RestoreSnapshot(QuestJournalSnapshot Snapshot);
}

// Runtime-only mutable state for one in-progress quest.
public class ActiveQuest
{
    public QuestData Definition;
    public int       CurrentStageIndex;
    public int[]     ObjectiveProgress;   // one counter per objective in the current stage
}
```

The `QuestManager`:

- Holds `Dictionary<string, ActiveQuest>` for active quests.
- On `StartQuest`: validates `Prerequisites`, creates an `ActiveQuest`, subscribes its current
  stage's objectives to the relevant EventBus events, raises `QuestStartedEvent`.
- When all objectives of a stage hit their `RequiredAmount`: applies `OnComplete` effects (which
  write to WorldState), advances `CurrentStageIndex`, re-subscribes for the new stage, raises
  `QuestStageChangedEvent`. Past the last stage → `QuestCompletedEvent`.
- Registered in Bootstrap; persisted via snapshot (active quest ids + stage index + counters).

### 6.4 Objective ↔ EventBus wiring

Objectives do not subscribe themselves (they live on the shared, immutable SO). The
`QuestManager` owns the subscriptions and asks each active objective whether an incoming event
advances it:

```csharp
// In QuestManager — one binding per gameplay event type, reused across all active quests.
private void HandleEnemyKilled(EnemyKilledEvent Event)
{
    foreach (ActiveQuest Quest in ActiveQuests.Values)
    {
        IReadOnlyList<QuestObjective> Objectives = Quest.Definition.Stages[Quest.CurrentStageIndex].Objectives;
        for (int Index = 0; Index < Objectives.Count; Index++)
        {
            if (Objectives[Index] is KillObjective Kill && Kill.Matches(Event.UnitId))
            {
                Quest.ObjectiveProgress[Index]++;
                TryCompleteStage(Quest);
            }
        }
    }
}
```

No LINQ, no per-frame allocation — the loop runs only on discrete gameplay events, never in
`Update()`.

---

## 7. Dialogue system — runtime model

The chosen direction: **a node graph**, authored in a custom GraphView editor, executed by a
runtime `DialogueRunner`. The runtime model is independent of the editor and is what gets
serialized.

### 7.1 Graph asset

```csharp
[CreateAssetMenu(menuName = "Valenthia/Narrative/Dialogue")]
public class DialogueData : ScriptableObject
{
    public string                       Id        => DialogueId;
    public string                       EntryGuid => EntryNodeGuid;
    public IReadOnlyList<DialogueNode>  Nodes     => GraphNodes;

    [SerializeField] private string DialogueId;
    [SerializeField] private string EntryNodeGuid;            // where the runner starts

    // The whole graph. Nodes reference each other by Guid (string), never by object reference,
    // so serialization is stable across renames/reorders and the editor can rebuild edges.
    [SerializeReference] private List<DialogueNode> GraphNodes = new();
}
```

### 7.2 Nodes (polymorphic)

```csharp
[Serializable]
public abstract class DialogueNode
{
    public string Guid     => NodeGuid;
    public Vector2 Position => EditorPosition;   // editor-only layout, ignored at runtime

    // Resolve this node and return the Guid of the next node to visit (or null to end).
    // Side effects (raising UI events, applying effects) happen inside.
    public abstract string Resolve(in DialogueExecution Execution);

    [SerializeField, HideInInspector] private string  NodeGuid;          // assigned on creation
    [SerializeField, HideInInspector] private Vector2 EditorPosition;
}
```

v1 node types:

| Node | Role | Outgoing |
|---|---|---|
| `StartNode` | Single entry point of the graph | 1 |
| `LineNode` | One spoken line: speaker id, text, optional portrait + voice clip | 1 |
| `ChoiceNode` | Presents player options; each choice has an optional `INarrativeCondition` and its own target | N (one per choice) |
| `ConditionNode` | Branches on `INarrativeCondition` (true / false ports) | 2 |
| `ActionNode` | Applies a list of `INarrativeEffect` then continues | 1 |
| `EndNode` | Terminates the conversation | 0 |

A node stores its outgoing links by target Guid (single nodes hold one; `ChoiceNode` holds one
per choice). The editor rebuilds visual edges from these Guids — there is no separate connection
list to keep in sync.

```csharp
[Serializable]
public class ChoiceNode : DialogueNode
{
    public override string Resolve(in DialogueExecution Execution)
    {
        // Filter choices whose condition fails, raise a UI event, await selection.
        Execution.PresentChoices(this);
        return null;   // the runner resumes from the selected choice's target (see §7.3)
    }

    public IReadOnlyList<DialogueChoice> Choices => ChoiceList;

    [SerializeField] private List<DialogueChoice> ChoiceList = new();
}

[Serializable]
public class DialogueChoice
{
    [SerializeField]     private string                  Text;
    [SerializeField]     private string                  TargetGuid;
    [SerializeReference] private List<INarrativeCondition> ShowIf = new();   // empty = always shown
}
```

### 7.3 The runner

`DialogueRunner` is a service (registered in Bootstrap) that walks the graph. It is fully
decoupled from the UI: it raises EventBus events and waits for input events back.

```csharp
public interface IDialogueRunner
{
    void StartDialogue(DialogueData Dialogue, Unit Initiator);
    void SubmitChoice(int ChoiceIndex);    // called when the UI reports a click
    void Advance();                        // called to continue past a LineNode
}
```

Flow:

1. `StartDialogue` builds a `NarrativeContext`, locates the `StartNode`, raises
   `DialogueStartedEvent`.
2. For each node, `Resolve` runs. `LineNode` raises `DialogueLineShownEvent { Speaker, Text,
   Portrait }` and **pauses** until the UI calls `Advance()`. `ChoiceNode` raises
   `DialogueChoicesShownEvent { Choices[] }` and pauses until `SubmitChoice`.
3. `ConditionNode` / `ActionNode` resolve immediately (no UI), `Action` applying its effects via
   the shared `NarrativeContext`.
4. On `EndNode` (or a null target) → `DialogueEndedEvent`.

Because the runner pauses on input rather than blocking a thread, there are no coroutines tied to
a GameObject lifetime (project gotcha: coroutines stop on disable) — state is a small
`DialogueExecution` struct the runner holds between events.

### 7.4 UI integration

A `DialoguePanel` MonoBehaviour in the persistent **UI** scene subscribes to the dialogue events
(`OnEnable` / `OnDisable`, binding kept — the project pattern) and renders the panel: portrait,
speaker name, body text, choice buttons. Player input (advance / select) routes through the New
Input System and calls back into `IDialogueRunner`. The runner never touches the UI directly.

---

## 8. The dialogue graph editor (GraphView)

The deliverable that makes this option B. Built entirely on Unity's native UI Toolkit /
`UnityEditor.Experimental.GraphView` — **no external package**.

### 8.1 Components

| Class | Responsibility |
|---|---|
| `DialogueGraphEditorWindow` | `EditorWindow`; opens on double-clicking a `DialogueData` asset; hosts the graph view + a toolbar (Save, Ping asset). |
| `DialogueGraphView` | The `GraphView` surface: pan/zoom, selection, copy/paste, edge connection rules, context menu. |
| `DialogueNodeView` | Visual `Node` for one `DialogueNode`: title, input/output ports, and an `IMGUIContainer` (or `PropertyField`) exposing the node's serialized fields — including the `[SerializeReference]` condition/effect lists via Unity's built-in managed-reference picker. |
| `DialogueSearchProvider` | `ISearchWindowProvider` for the right-click "Add node" menu, listing every `DialogueNode` subtype via reflection. |
| `DialogueGraphSerializer` | Maps the `GraphView` (node views + edges) ⇄ the `DialogueData` asset. The only class that knows both worlds. |

### 8.2 Serialization strategy

- The asset stores nodes as a flat `[SerializeReference] List<DialogueNode>`; connectivity lives
  in each node's target Guid(s). No sub-assets, no separate connection list.
- On **save**, the serializer walks node views: writes back each node's `EditorPosition`, updates
  target Guids from the live edges, marks the `DialogueData` dirty (`EditorUtility.SetDirty` +
  `AssetDatabase.SaveAssets`).
- On **load**, it instantiates one `DialogueNodeView` per node at its stored position, then draws
  an edge for every target Guid. Unknown/dangling Guids are surfaced as a validation warning, not
  a silent drop.
- A `Validate` pass (run on save) flags: no `StartNode`, multiple `StartNode`s, unreachable
  nodes, `ChoiceNode` with a choice pointing nowhere, empty `LineNode` text.

### 8.3 Two-phase delivery (de-risking)

The model in §7 is authorable without the window, so we build in two phases and never block on
tooling:

- **Phase A — data + runtime.** `DialogueData`, all node types, `DialogueRunner`, `DialoguePanel`,
  events. Author a test conversation by adding nodes through the Inspector's managed-reference
  picker (tedious but functional). Proves the runtime end-to-end.
- **Phase B — the GraphView window.** Layer the visual editor over the exact same asset. No
  runtime change. This is the bulk of the editor effort and can land after the first quests ship.

---

## 9. EventBus catalogue (`NarrativeEvents.cs`)

All structs implement `IEvent`. Publishers/subscribers per the project pattern (binding kept,
`Register`/`Deregister` in `OnEnable`/`OnDisable`).

| Event | Raised by | Consumed by |
|---|---|---|
| `WorldStateChangedEvent { string Key }` | WorldStateTracker | reactive NPCs, UI, debug overlay |
| `ReputationChangedEvent { FactionId Faction, int NewValue }` | ReputationSystem | UI, dialogue gating refresh |
| `QuestStartedEvent { string QuestId }` | QuestManager | journal UI, audio |
| `QuestStageChangedEvent { string QuestId, int StageIndex }` | QuestManager | journal UI, objective tracker |
| `QuestCompletedEvent { string QuestId }` | QuestManager | journal UI, reward VFX/audio |
| `EnemyKilledEvent { string UnitId }` | combat | QuestManager (KillObjective) |
| `ItemCollectedEvent { string ItemId, int Count }` | inventory | QuestManager (CollectObjective) |
| `ZoneEnteredEvent { string ZoneId }` | SceneLoader | QuestManager (ReachObjective) |
| `DialogueStartedEvent { string DialogueId }` | DialogueRunner | UI, input mode switch, camera |
| `DialogueLineShownEvent { string Speaker, string Text, Sprite Portrait }` | DialogueRunner | DialoguePanel |
| `DialogueChoicesShownEvent { DialogueChoiceView[] Choices }` | DialogueRunner | DialoguePanel |
| `DialogueEndedEvent { string DialogueId }` | DialogueRunner | UI, input mode restore |

> Some of these (`EnemyKilledEvent`, `ItemCollectedEvent`, `ZoneEnteredEvent`) belong to other
> modules and may already exist or be added in those modules' own `XxxEvents.cs` — the narrative
> stack only consumes them.

---

## 10. Save integration

The SaveSystem (architecture still pending) serializes exactly three narrative snapshots:

1. `WorldStateSnapshot` — all flags/vars (includes reputation, since it's WorldState-backed).
2. `QuestJournalSnapshot` — active quest ids, current stage index, objective counters.
3. Dialogue needs **no** snapshot of its own — "seen this line / made this choice" is recorded as
   world flags by `ActionNode`s, so it rides along in the WorldStateSnapshot.

This is the payoff of "WorldState is the single source of truth": save/load touches three structs,
not every system.

---

## 11. Implementation order

| Step | Deliverable | Depends on |
|---|---|---|
| 1 | `NarrativeDefines`, `NarrativeContext`, condition/effect bases + v1 concretes | — |
| 2 | `WorldStateTracker` + `IWorldStateTracker`, `WorldStateChangedEvent` | 1 |
| 3 | `ReputationSystem` | 2 |
| 4 | Quest model (`QuestData`, objectives, `QuestManager`) + EventBus wiring | 1–2 |
| 5 | Dialogue runtime (`DialogueData`, nodes, `DialogueRunner`) — **Phase A** | 1–2 |
| 6 | `DialoguePanel` UI + input | 5 |
| 7 | GraphView editor — **Phase B** | 5 |
| 8 | SaveSystem snapshots wired in | 2, 4 |

Steps 1–2 are the foundation everything else needs; build them first.

---

## 12. Open decisions

- **Speaker identity**: `LineNode.Speaker` as a free string id vs a `DT_Actor` SO (portrait, voice
  set, display name). Leaning `DT_Actor` for reuse, but string is fine for Phase A.
- **Localization**: line text inline now; a `LocalizationKey` indirection later if/when a
  localization pass happens. Not Phase A.
- **Variable interpolation** in line text (`"Hello {player_name}"`): deferred; add a single
  resolve pass in `DialoguePanel` when needed.
- **Blackboard / exposed variables** in the GraphView (for per-conversation locals): nice-to-have,
  out of Phase B scope unless authoring pain demands it.
