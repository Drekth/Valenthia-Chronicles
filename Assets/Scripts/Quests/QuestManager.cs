using System.Collections.Generic;
using UnityEngine;

// Contract for the quest journal. Resolved through the ServiceLocator (registered in Bootstrap),
// so dialogue nodes and UI never reference the concrete manager.
public interface IQuestManager
{
    void        StartQuest(QuestData Quest);
    void        CompleteQuest(string QuestId);
    QuestStatus GetStatus(string QuestId);
    bool        CanOffer(QuestData Quest);
    bool        IsReadyToTurnIn(string QuestId);
    ActiveQuest GetActive(string QuestId);
}

// Runtime-only mutable progress for one quest. The QuestData definition stays immutable.
public class ActiveQuest
{
    public QuestData   Definition;
    public int         CurrentStageIndex;
    public int[]       ObjectiveProgress;
    public QuestStatus Status;
}

// Owns every known quest and the EventBus subscriptions that advance their objectives. It asks
// each active objective whether an incoming gameplay event advances it (no LINQ, no per-frame
// allocation — the loop runs only on discrete events). Lives on the persistent Bootstrap scene.
public class QuestManager : MonoBehaviour, IQuestManager
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    // Begins tracking a quest the player just accepted. Ignored if it is already known.
    public void StartQuest(QuestData Quest)
    {
        if (Quest == null || !CanOffer(Quest))
        {
            return;
        }

        QuestStage FirstStage = GetStage(Quest, 0);
        ActiveQuest Active = new ActiveQuest
        {
            Definition        = Quest,
            CurrentStageIndex = 0,
            ObjectiveProgress = new int[FirstStage != null ? FirstStage.Objectives.Count : 0],
            Status            = QuestStatus.Active,
        };
        Quests[Quest.Id] = Active;

        EventBus<QuestStartedEvent>.Raise(new QuestStartedEvent { QuestId = Quest.Id });

        // A first stage with no objectives is already done — fall straight through to turn-in.
        TryAdvanceStage(Active);
    }

    // Hands a quest in at its giver. Only valid once every objective is complete.
    public void CompleteQuest(string QuestId)
    {
        if (string.IsNullOrEmpty(QuestId) || !Quests.TryGetValue(QuestId, out ActiveQuest Active))
        {
            return;
        }

        if (Active.Status != QuestStatus.ReadyToTurnIn)
        {
            return;
        }

        Active.Status = QuestStatus.Completed;

        // TODO Rewards: grant Active.Definition.Rewards once an inventory grant API exists. For now
        // the rewards are shown on the turn-in panel and announced through the event below.

        EventBus<QuestCompletedEvent>.Raise(new QuestCompletedEvent { QuestId = QuestId });
    }

    public QuestStatus GetStatus(string QuestId)
    {
        if (!string.IsNullOrEmpty(QuestId) && Quests.TryGetValue(QuestId, out ActiveQuest Active))
        {
            return Active.Status;
        }
        return QuestStatus.None;
    }

    public bool CanOffer(QuestData Quest)
    {
        return Quest != null && GetStatus(Quest.Id) == QuestStatus.None;
    }

    public bool IsReadyToTurnIn(string QuestId)
    {
        return GetStatus(QuestId) == QuestStatus.ReadyToTurnIn;
    }

    public ActiveQuest GetActive(string QuestId)
    {
        if (!string.IsNullOrEmpty(QuestId) && Quests.TryGetValue(QuestId, out ActiveQuest Active))
        {
            return Active;
        }
        return null;
    }

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void Awake()
    {
        ServiceLocator.Register<IQuestManager>(this);
    }

    private void OnEnable()
    {
        UnitDiedBinding = new EventBinding<UnitDiedEvent>(HandleUnitDied);
        EventBus<UnitDiedEvent>.Register(UnitDiedBinding);
    }

    private void OnDisable()
    {
        EventBus<UnitDiedEvent>.Deregister(UnitDiedBinding);
    }

    private void OnDestroy()
    {
        if (ServiceLocator.TryGet<IQuestManager>(out IQuestManager Current) && ReferenceEquals(Current, this))
        {
            ServiceLocator.Unregister<IQuestManager>();
        }
    }

    // Advances a kill objective for every active quest whose current stage tracks the dead unit.
    private void HandleUnitDied(UnitDiedEvent Event)
    {
        if (Event.Unit == null)
        {
            return;
        }

        string UnitId = Event.Unit.Id;
        if (string.IsNullOrEmpty(UnitId))
        {
            return;
        }

        foreach (ActiveQuest Active in Quests.Values)
        {
            if (Active.Status != QuestStatus.Active)
            {
                continue;
            }

            QuestStage Stage = GetStage(Active.Definition, Active.CurrentStageIndex);
            if (Stage == null)
            {
                continue;
            }

            IReadOnlyList<QuestObjective> Objectives = Stage.Objectives;
            bool Progressed = false;
            for (int Index = 0; Index < Objectives.Count; Index++)
            {
                if (Objectives[Index] is KillObjective Kill
                    && Kill.Matches(UnitId)
                    && Active.ObjectiveProgress[Index] < Kill.RequiredAmount)
                {
                    Active.ObjectiveProgress[Index]++;
                    Progressed = true;

                    EventBus<QuestObjectiveProgressedEvent>.Raise(new QuestObjectiveProgressedEvent
                    {
                        QuestId        = Active.Definition.Id,
                        StageIndex     = Active.CurrentStageIndex,
                        ObjectiveIndex = Index,
                        Current        = Active.ObjectiveProgress[Index],
                        Required       = Kill.RequiredAmount,
                    });
                }
            }

            if (Progressed)
            {
                TryAdvanceStage(Active);
            }
        }
    }

    // Steps a quest forward while its current stage is complete: to the next stage, or to
    // ReadyToTurnIn once the last stage is done. Cascades through empty stages in one call.
    private void TryAdvanceStage(ActiveQuest Active)
    {
        while (Active.Status == QuestStatus.Active && IsStageComplete(Active))
        {
            int LastIndex = Active.Definition.Stages.Count - 1;
            if (Active.CurrentStageIndex < LastIndex)
            {
                Active.CurrentStageIndex++;
                QuestStage Stage = Active.Definition.Stages[Active.CurrentStageIndex];
                Active.ObjectiveProgress = new int[Stage.Objectives.Count];

                EventBus<QuestStageChangedEvent>.Raise(new QuestStageChangedEvent
                {
                    QuestId    = Active.Definition.Id,
                    StageIndex = Active.CurrentStageIndex,
                });
            }
            else
            {
                Active.Status = QuestStatus.ReadyToTurnIn;

                EventBus<QuestReadyToTurnInEvent>.Raise(new QuestReadyToTurnInEvent
                {
                    QuestId = Active.Definition.Id,
                });
            }
        }
    }

    private bool IsStageComplete(ActiveQuest Active)
    {
        QuestStage Stage = GetStage(Active.Definition, Active.CurrentStageIndex);
        if (Stage == null)
        {
            return true;
        }

        IReadOnlyList<QuestObjective> Objectives = Stage.Objectives;
        for (int Index = 0; Index < Objectives.Count; Index++)
        {
            if (Active.ObjectiveProgress[Index] < Objectives[Index].RequiredAmount)
            {
                return false;
            }
        }
        return true;
    }

    private static QuestStage GetStage(QuestData Quest, int Index)
    {
        if (Quest == null || Index < 0 || Index >= Quest.Stages.Count)
        {
            return null;
        }
        return Quest.Stages[Index];
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    private readonly Dictionary<string, ActiveQuest> Quests = new Dictionary<string, ActiveQuest>();

    private EventBinding<UnitDiedEvent> UnitDiedBinding;
}
