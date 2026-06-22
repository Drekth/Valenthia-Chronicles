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
    }

    protected override void OnUnsubscribe()
    {
        EventBus<DialogueStartedEvent>.Deregister(StartedBinding);
        EventBus<DialogueLineShownEvent>.Deregister(LineBinding);
        EventBus<DialogueChoicesShownEvent>.Deregister(ChoicesBinding);
        EventBus<DialogueEndedEvent>.Deregister(EndedBinding);
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
}
