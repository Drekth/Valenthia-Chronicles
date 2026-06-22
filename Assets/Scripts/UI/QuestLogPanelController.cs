using System.Collections.Generic;
using UnityEngine.UIElements;

// Openable quest journal (WoW-style), toggled by a key (see ToggleBinding). Master-detail layout:
// the left list holds every active quest, clicking one shows its description, objective progress and
// rewards on the right. Window chrome and Show/Hide/toggle lifecycle come from UIWindowController.
// Like the persistent QuestTracker it is purely event-driven — it repaints on quest EventBus
// channels and on each open, never polling. Quest detail is read from the IQuestManager.
public class QuestLogPanelController : UIWindowController
{
    ////////////////////////////////////////////////////////////
    /// Constants                                            ///
    ////////////////////////////////////////////////////////////

    private const string EntryClass         = "questlog__entry";
    private const string EntrySelectedClass = "questlog__entry--selected";
    private const string EntryReadyClass    = "questlog__entry--ready";
    private const string ObjectiveClass     = "questlog__objective";
    private const string ObjectiveDoneClass = "questlog__objective--done";
    private const string RewardClass        = "questlog__reward";
    private const string ReadyText          = "Prête à rendre — retournez voir le donneur.";

    ////////////////////////////////////////////////////////////
    /// Protected                                            ///
    ////////////////////////////////////////////////////////////

    protected override void OnBindContent(VisualElement Root)
    {
        ListContainer       = Root.Q<VisualElement>("QuestList");
        EmptyHint           = Root.Q<Label>("EmptyHint");
        DetailScroll        = Root.Q<VisualElement>("QuestDetailScroll");
        DetailTitle         = Root.Q<Label>("DetailTitle");
        DetailStatus        = Root.Q<Label>("DetailStatus");
        DetailSummary       = Root.Q<Label>("DetailSummary");
        ObjectivesHeader    = Root.Q<Label>("ObjectivesHeader");
        ObjectivesContainer = Root.Q<VisualElement>("DetailObjectives");
        RewardsHeader       = Root.Q<Label>("RewardsHeader");
        RewardsContainer    = Root.Q<VisualElement>("DetailRewards");
    }

    protected override void OnSubscribe()
    {
        StartedBinding = new EventBinding<QuestStartedEvent>(HandleChanged);
        EventBus<QuestStartedEvent>.Register(StartedBinding);

        ProgressedBinding = new EventBinding<QuestObjectiveProgressedEvent>(HandleChanged);
        EventBus<QuestObjectiveProgressedEvent>.Register(ProgressedBinding);

        StageChangedBinding = new EventBinding<QuestStageChangedEvent>(HandleChanged);
        EventBus<QuestStageChangedEvent>.Register(StageChangedBinding);

        ReadyBinding = new EventBinding<QuestReadyToTurnInEvent>(HandleChanged);
        EventBus<QuestReadyToTurnInEvent>.Register(ReadyBinding);

        CompletedBinding = new EventBinding<QuestCompletedEvent>(HandleChanged);
        EventBus<QuestCompletedEvent>.Register(CompletedBinding);
    }

    protected override void OnUnsubscribe()
    {
        EventBus<QuestStartedEvent>.Deregister(StartedBinding);
        EventBus<QuestObjectiveProgressedEvent>.Deregister(ProgressedBinding);
        EventBus<QuestStageChangedEvent>.Deregister(StageChangedBinding);
        EventBus<QuestReadyToTurnInEvent>.Deregister(ReadyBinding);
        EventBus<QuestCompletedEvent>.Deregister(CompletedBinding);
    }

    protected override void Rebuild()
    {
        if (ListContainer == null)
        {
            return;
        }

        ListContainer.Clear();

        IQuestManager Quests = ResolveQuests();
        ActiveQuest Selected = null;
        bool SelectionStillActive = false;

        if (Quests != null)
        {
            foreach (ActiveQuest Active in Quests.AllQuests)
            {
                if (!IsActive(Active))
                {
                    continue;
                }

                AddListEntry(Active);

                if (Selected == null)
                {
                    Selected = Active;
                }

                if (Active.Definition.Id == SelectedQuestId)
                {
                    SelectionStillActive = true;
                }
            }
        }

        bool HasQuests = ListContainer.childCount > 0;
        SetDisplay(EmptyHint, !HasQuests);
        SetDisplay(DetailScroll, HasQuests);

        if (!HasQuests)
        {
            SelectedQuestId = null;
            return;
        }

        // Keep the previous selection when it is still active, otherwise fall back to the first.
        if (SelectionStillActive)
        {
            Selected = FindActive(Quests, SelectedQuestId);
        }

        SelectedQuestId = Selected.Definition.Id;
        RefreshSelection(Selected);
    }

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    // Started / progressed / stage / ready / completed all repaint the same panel, so they share
    // one handler. Only repaint while open — Show() rebuilds on the next open otherwise.
    private void HandleChanged()
    {
        if (IsShown)
        {
            Rebuild();
        }
    }

    private void AddListEntry(ActiveQuest Active)
    {
        string QuestId = Active.Definition.Id;

        Button Entry = new Button();
        Entry.text = Active.Definition.Title;
        Entry.userData = QuestId;
        Entry.AddToClassList(EntryClass);

        if (Active.Status == QuestStatus.ReadyToTurnIn)
        {
            Entry.AddToClassList(EntryReadyClass);
        }

        Entry.clicked += () => HandleEntryClicked(QuestId);

        ListContainer.Add(Entry);
    }

    private void HandleEntryClicked(string QuestId)
    {
        SelectedQuestId = QuestId;

        ActiveQuest Active = FindActive(ResolveQuests(), QuestId);
        if (Active != null)
        {
            RefreshSelection(Active);
        }
    }

    // Highlights the selected list entry and fills the detail pane from its current state.
    private void RefreshSelection(ActiveQuest Selected)
    {
        for (int Index = 0; Index < ListContainer.childCount; Index++)
        {
            VisualElement Entry = ListContainer[Index];
            bool IsSelected = Entry.userData is string EntryId && EntryId == Selected.Definition.Id;
            Entry.EnableInClassList(EntrySelectedClass, IsSelected);
        }

        ShowDetail(Selected);
    }

    private void ShowDetail(ActiveQuest Active)
    {
        DetailTitle.text   = Active.Definition.Title;
        DetailSummary.text = Active.Definition.Summary;

        bool Ready = Active.Status == QuestStatus.ReadyToTurnIn;
        DetailStatus.text = Ready ? ReadyText : string.Empty;
        SetDisplay(DetailStatus, Ready);

        BuildObjectives(Active, Ready);
        BuildRewards(Active);
    }

    private void BuildObjectives(ActiveQuest Active, bool Ready)
    {
        ObjectivesContainer.Clear();

        QuestStage Stage = GetCurrentStage(Active);

        // A quest ready to turn in has no remaining objectives to show; hide the section entirely.
        bool ShowObjectives = !Ready && Stage != null && Stage.Objectives.Count > 0;
        SetDisplay(ObjectivesHeader, ShowObjectives);
        SetDisplay(ObjectivesContainer, ShowObjectives);

        if (!ShowObjectives)
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
            if (Current >= Required)
            {
                Line.AddToClassList(ObjectiveDoneClass);
            }
            ObjectivesContainer.Add(Line);
        }
    }

    private void BuildRewards(ActiveQuest Active)
    {
        RewardsContainer.Clear();

        IReadOnlyList<QuestReward> Rewards = Active.Definition.Rewards;
        int Shown = 0;

        for (int Index = 0; Index < Rewards.Count; Index++)
        {
            string Text = Rewards[Index].Describe();
            if (string.IsNullOrEmpty(Text))
            {
                continue;
            }

            Label Line = new Label(Text);
            Line.AddToClassList(RewardClass);
            RewardsContainer.Add(Line);
            Shown++;
        }

        SetDisplay(RewardsHeader, Shown > 0);
        SetDisplay(RewardsContainer, Shown > 0);
    }

    private static bool IsActive(ActiveQuest Active)
    {
        return Active != null
            && Active.Definition != null
            && (Active.Status == QuestStatus.Active || Active.Status == QuestStatus.ReadyToTurnIn);
    }

    private static ActiveQuest FindActive(IQuestManager Quests, string QuestId)
    {
        if (Quests == null || string.IsNullOrEmpty(QuestId))
        {
            return null;
        }

        ActiveQuest Active = Quests.GetActive(QuestId);
        return IsActive(Active) ? Active : null;
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

    private static void SetDisplay(VisualElement Element, bool Visible)
    {
        if (Element != null)
        {
            Element.style.display = Visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    private static IQuestManager ResolveQuests()
    {
        ServiceLocator.TryGet<IQuestManager>(out IQuestManager Manager);
        return Manager;
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    private VisualElement ListContainer;
    private Label         EmptyHint;
    private VisualElement DetailScroll;
    private Label         DetailTitle;
    private Label         DetailStatus;
    private Label         DetailSummary;
    private Label         ObjectivesHeader;
    private VisualElement ObjectivesContainer;
    private Label         RewardsHeader;
    private VisualElement RewardsContainer;

    private string SelectedQuestId;

    private EventBinding<QuestStartedEvent>             StartedBinding;
    private EventBinding<QuestObjectiveProgressedEvent> ProgressedBinding;
    private EventBinding<QuestStageChangedEvent>        StageChangedBinding;
    private EventBinding<QuestReadyToTurnInEvent>       ReadyBinding;
    private EventBinding<QuestCompletedEvent>           CompletedBinding;
}
