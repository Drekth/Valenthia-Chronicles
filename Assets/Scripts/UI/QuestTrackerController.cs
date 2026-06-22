using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// Persistent on-screen quest tracker (WoW-style), built with UI Toolkit (QuestTracker.uxml/.uss)
// and living in the persistent UI scene. Listens to the quest EventBus channels and repaints the
// list of active quests with their objective progress — it never polls. Quest detail is read from
// the IQuestManager on each repaint, which only happens on a discrete quest event.
[RequireComponent(typeof(UIDocument))]
public class QuestTrackerController : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Constants                                            ///
    ////////////////////////////////////////////////////////////

    private const string TitleClass     = "quest-title";
    private const string ObjectiveClass = "quest-objective";
    private const string ReadyClass     = "quest-ready";
    private const string ReadyText      = "Prête à rendre — retournez voir le donneur";

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void Awake()
    {
        Document      = GetComponent<UIDocument>();
        TrackedQuests = new List<string>();
    }

    private void OnEnable()
    {
        BindVisualTree();

        StartedBinding = new EventBinding<QuestStartedEvent>(HandleStarted);
        EventBus<QuestStartedEvent>.Register(StartedBinding);

        ProgressedBinding = new EventBinding<QuestObjectiveProgressedEvent>(HandleChanged);
        EventBus<QuestObjectiveProgressedEvent>.Register(ProgressedBinding);

        StageChangedBinding = new EventBinding<QuestStageChangedEvent>(HandleChanged);
        EventBus<QuestStageChangedEvent>.Register(StageChangedBinding);

        ReadyBinding = new EventBinding<QuestReadyToTurnInEvent>(HandleChanged);
        EventBus<QuestReadyToTurnInEvent>.Register(ReadyBinding);

        CompletedBinding = new EventBinding<QuestCompletedEvent>(HandleCompleted);
        EventBus<QuestCompletedEvent>.Register(CompletedBinding);

        Rebuild();
    }

    private void OnDisable()
    {
        EventBus<QuestStartedEvent>.Deregister(StartedBinding);
        EventBus<QuestObjectiveProgressedEvent>.Deregister(ProgressedBinding);
        EventBus<QuestStageChangedEvent>.Deregister(StageChangedBinding);
        EventBus<QuestReadyToTurnInEvent>.Deregister(ReadyBinding);
        EventBus<QuestCompletedEvent>.Deregister(CompletedBinding);
    }

    private void HandleStarted(QuestStartedEvent Event)
    {
        if (!TrackedQuests.Contains(Event.QuestId))
        {
            TrackedQuests.Add(Event.QuestId);
        }
        Rebuild();
    }

    // Progress / stage / ready-to-turn-in all repaint the same list, so they share one handler.
    private void HandleChanged()
    {
        Rebuild();
    }

    private void HandleCompleted(QuestCompletedEvent Event)
    {
        TrackedQuests.Remove(Event.QuestId);
        Rebuild();
    }

    private void BindVisualTree()
    {
        VisualElement Root = Document.rootVisualElement;
        ListContainer = Root != null ? Root.Q<VisualElement>("QuestList") : null;
        HeaderLabel   = Root != null ? Root.Q<Label>("QuestTrackerHeader") : null;
    }

    private void Rebuild()
    {
        if (ListContainer == null)
        {
            return;
        }

        ListContainer.Clear();

        IQuestManager Quests = ResolveQuests();
        int Shown = 0;

        if (Quests != null)
        {
            for (int Index = 0; Index < TrackedQuests.Count; Index++)
            {
                ActiveQuest Active = Quests.GetActive(TrackedQuests[Index]);
                if (Active == null || Active.Definition == null)
                {
                    continue;
                }

                AddQuestEntry(Active);
                Shown++;
            }
        }

        // Hide the header when there is nothing to track, so the corner stays clean.
        if (HeaderLabel != null)
        {
            HeaderLabel.style.display = Shown > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    private void AddQuestEntry(ActiveQuest Active)
    {
        Label Title = new Label(Active.Definition.Title);
        Title.AddToClassList(TitleClass);
        Title.pickingMode = PickingMode.Ignore;
        ListContainer.Add(Title);

        if (Active.Status == QuestStatus.ReadyToTurnIn)
        {
            Label Ready = new Label(ReadyText);
            Ready.AddToClassList(ReadyClass);
            Ready.pickingMode = PickingMode.Ignore;
            ListContainer.Add(Ready);
            return;
        }

        QuestStage Stage = GetCurrentStage(Active);
        if (Stage == null)
        {
            return;
        }

        IReadOnlyList<QuestObjective> Objectives = Stage.Objectives;
        for (int Index = 0; Index < Objectives.Count; Index++)
        {
            int Current  = Index < Active.ObjectiveProgress.Length ? Active.ObjectiveProgress[Index] : 0;
            int Required = Objectives[Index].RequiredAmount;

            Label Line = new Label($"{Objectives[Index].Describe()} : {Current}/{Required}");
            Line.AddToClassList(ObjectiveClass);
            Line.pickingMode = PickingMode.Ignore;
            ListContainer.Add(Line);
        }
    }

    private static QuestStage GetCurrentStage(ActiveQuest Active)
    {
        IReadOnlyList<QuestStage> Stages = Active.Definition.Stages;
        if (Active.CurrentStageIndex < 0 || Active.CurrentStageIndex >= Stages.Count)
        {
            return null;
        }
        return Stages[Active.CurrentStageIndex];
    }

    private static IQuestManager ResolveQuests()
    {
        ServiceLocator.TryGet<IQuestManager>(out IQuestManager Manager);
        return Manager;
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    private UIDocument    Document;
    private VisualElement ListContainer;
    private Label         HeaderLabel;

    private List<string>  TrackedQuests;

    private EventBinding<QuestStartedEvent>             StartedBinding;
    private EventBinding<QuestObjectiveProgressedEvent> ProgressedBinding;
    private EventBinding<QuestStageChangedEvent>        StageChangedBinding;
    private EventBinding<QuestReadyToTurnInEvent>       ReadyBinding;
    private EventBinding<QuestCompletedEvent>           CompletedBinding;
}
