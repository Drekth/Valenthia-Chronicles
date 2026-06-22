// Dialogue domain events, published by the DialogueRunner and consumed by the DialoguePanel UI.
// The runner never touches the UI directly: it raises these and waits for the UI to call back
// (Advance / SubmitChoice) — same decoupling as the loot window over the container channels.

// Announces that a conversation has begun, carrying the speaking NPC's display name so the panel
// can title itself before the first line arrives.
public struct DialogueStartedEvent : IEvent
{
    public string DialogueId;
    public string NpcName;
}

// One spoken line. IsFinal is true when advancing past it ends the conversation, so the UI can
// label its button "Au revoir" instead of "Continuer".
public struct DialogueLineShownEvent : IEvent
{
    public string Speaker;
    public string Text;
    public bool   IsFinal;
}

// Presents the player's options for the current ChoiceNode. The array is allocated only when a
// choice is shown (on player input), never per frame.
public struct DialogueChoicesShownEvent : IEvent
{
    public DialogueOption[] Options;
}

// Notifies that the conversation has ended (no payload).
public struct DialogueEndedEvent : IEvent
{
}

// Presents a WoW-style quest offer (IsTurnIn false) or hand-in (IsTurnIn true) inside the dialogue
// panel. The runner pauses on a QuestOfferNode / QuestTurnInNode, raises this, and waits for the
// panel to call back through DialogueRunner.SubmitChoice — 0 = accept / complete, 1 = decline /
// later — exactly the channel a ChoiceNode uses. Payload is strings so the panel stays decoupled
// from QuestData.
public struct DialogueQuestOfferedEvent : IEvent
{
    public string   QuestTitle;
    public string   Summary;
    public string[] Objectives;
    public string[] Rewards;
    public bool     IsTurnIn;
}

// One selectable option carried in DialogueChoicesShownEvent: the text to display and the index the
// UI passes back to DialogueRunner.SubmitChoice.
public struct DialogueOption
{
    public int    Index;
    public string Text;
}
