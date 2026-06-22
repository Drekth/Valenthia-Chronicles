using UnityEngine.UIElements;

// Drives the dialogue panel. Event-driven (no toggle key): the DialogueRunner raises the dialogue
// channels, this controller shows the panel, paints the current line or the choice buttons, and
// routes clicks back into the runner (Advance / SubmitChoice / Abort). Window chrome and Show/Hide
// lifecycle come from UIWindowController. The runner never touches the UI directly.
public class DialoguePanelController : UIWindowController
{
    ////////////////////////////////////////////////////////////
    /// Protected                                            ///
    ////////////////////////////////////////////////////////////

    protected override void OnBindContent(VisualElement Root)
    {
        PanelWindow = Root.Q<UIWindow>("DialogueWindow");
        BodyLabel   = Root.Q<Label>("Body");
        OptionsBox  = Root.Q<VisualElement>("Options");

        // Closing the panel (the X) cancels the conversation so the runner resets and a new
        // right-click can start a fresh one. The base also hides on close.
        if (PanelWindow != null)
        {
            PanelWindow.CloseRequested += AbortDialogue;
        }
    }

    protected override void OnSubscribe()
    {
        StartedBinding = new EventBinding<DialogueStartedEvent>(HandleStarted);
        EventBus<DialogueStartedEvent>.Register(StartedBinding);

        LineBinding = new EventBinding<DialogueLineShownEvent>(HandleLineShown);
        EventBus<DialogueLineShownEvent>.Register(LineBinding);

        ChoicesBinding = new EventBinding<DialogueChoicesShownEvent>(HandleChoicesShown);
        EventBus<DialogueChoicesShownEvent>.Register(ChoicesBinding);

        EndedBinding = new EventBinding<DialogueEndedEvent>(HandleEnded);
        EventBus<DialogueEndedEvent>.Register(EndedBinding);

        QuestOfferedBinding = new EventBinding<DialogueQuestOfferedEvent>(HandleQuestOffered);
        EventBus<DialogueQuestOfferedEvent>.Register(QuestOfferedBinding);
    }

    protected override void OnUnsubscribe()
    {
        EventBus<DialogueStartedEvent>.Deregister(StartedBinding);
        EventBus<DialogueLineShownEvent>.Deregister(LineBinding);
        EventBus<DialogueChoicesShownEvent>.Deregister(ChoicesBinding);
        EventBus<DialogueEndedEvent>.Deregister(EndedBinding);
        EventBus<DialogueQuestOfferedEvent>.Deregister(QuestOfferedBinding);
    }

    // Show() resets the panel before the first event paints it.
    protected override void Rebuild()
    {
        ClearOptions();

        if (BodyLabel != null)
        {
            BodyLabel.text = string.Empty;
        }
    }

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void HandleStarted(DialogueStartedEvent Event)
    {
        if (PanelWindow != null)
        {
            PanelWindow.Title = Event.NpcName;
        }

        Show();
    }

    private void HandleLineShown(DialogueLineShownEvent Event)
    {
        if (BodyLabel != null)
        {
            BodyLabel.text = Event.Text;
        }

        ClearOptions();

        // A single advance button — labelled "Au revoir" when it will close the conversation.
        string Label = Event.IsFinal ? "Au revoir" : "Continuer";
        AddOption(Label, OnContinueClicked);
    }

    private void HandleChoicesShown(DialogueChoicesShownEvent Event)
    {
        // The preceding line stays in the body (WoW gossip feel); only the options are rebuilt.
        ClearOptions();

        if (Event.Options == null)
        {
            return;
        }

        for (int Index = 0; Index < Event.Options.Length; Index++)
        {
            DialogueOption Option = Event.Options[Index];
            int CapturedIndex = Option.Index;
            AddOption(Option.Text, () => SubmitChoice(CapturedIndex));
        }
    }

    private void HandleEnded()
    {
        Hide();
    }

    // A quest offer / hand-in pauses the conversation: the body shows the quest details and the
    // option bar becomes Accept/Decline (or Complete/Later). The buttons route through the same
    // SubmitChoice channel as ordinary choices (0 = accept/complete, 1 = decline/later).
    private void HandleQuestOffered(DialogueQuestOfferedEvent Event)
    {
        if (BodyLabel != null)
        {
            BodyLabel.text = BuildQuestBody(Event);
        }

        ClearOptions();

        if (Event.IsTurnIn)
        {
            AddOption("Terminer la quête", () => SubmitChoice(0));
            AddOption("Plus tard", () => SubmitChoice(1));
        }
        else
        {
            AddOption("Accepter", () => SubmitChoice(0));
            AddOption("Refuser", () => SubmitChoice(1));
        }
    }

    private static string BuildQuestBody(DialogueQuestOfferedEvent Event)
    {
        System.Text.StringBuilder Builder = new System.Text.StringBuilder();
        Builder.Append(Event.QuestTitle);

        if (!string.IsNullOrEmpty(Event.Summary))
        {
            Builder.Append("\n\n");
            Builder.Append(Event.Summary);
        }

        if (Event.Objectives != null && Event.Objectives.Length > 0)
        {
            Builder.Append("\n\nObjectifs :");
            for (int Index = 0; Index < Event.Objectives.Length; Index++)
            {
                Builder.Append("\n- ");
                Builder.Append(Event.Objectives[Index]);
            }
        }

        if (Event.Rewards != null && Event.Rewards.Length > 0)
        {
            Builder.Append("\n\nRécompenses :");
            for (int Index = 0; Index < Event.Rewards.Length; Index++)
            {
                Builder.Append("\n- ");
                Builder.Append(Event.Rewards[Index]);
            }
        }

        return Builder.ToString();
    }

    private void AddOption(string Text, System.Action OnClick)
    {
        if (OptionsBox == null)
        {
            return;
        }

        Button OptionButton = new Button(OnClick) { text = Text };
        OptionButton.AddToClassList(OptionClass);
        OptionsBox.Add(OptionButton);
    }

    private void ClearOptions()
    {
        if (OptionsBox != null)
        {
            OptionsBox.Clear();
        }
    }

    private void OnContinueClicked()
    {
        if (ServiceLocator.TryGet<DialogueRunner>(out DialogueRunner Runner))
        {
            Runner.Advance();
        }
    }

    private void SubmitChoice(int Index)
    {
        if (ServiceLocator.TryGet<DialogueRunner>(out DialogueRunner Runner))
        {
            Runner.SubmitChoice(Index);
        }
    }

    private void AbortDialogue()
    {
        if (ServiceLocator.TryGet<DialogueRunner>(out DialogueRunner Runner))
        {
            Runner.Abort();
        }
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    private const string OptionClass = "dialogue-option";

    private UIWindow      PanelWindow;
    private Label         BodyLabel;
    private VisualElement OptionsBox;

    private EventBinding<DialogueStartedEvent>      StartedBinding;
    private EventBinding<DialogueLineShownEvent>     LineBinding;
    private EventBinding<DialogueChoicesShownEvent>  ChoicesBinding;
    private EventBinding<DialogueEndedEvent>          EndedBinding;
    private EventBinding<DialogueQuestOfferedEvent>   QuestOfferedBinding;
}
