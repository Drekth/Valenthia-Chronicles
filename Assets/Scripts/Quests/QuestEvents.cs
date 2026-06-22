// Quest domain events, published by the QuestManager and consumed by the quest tracker UI (and
// later audio / reward VFX). Same EventBus pattern as the combat and dialogue modules.

// Raised when a quest is accepted and starts being tracked.
public struct QuestStartedEvent : IEvent
{
    public string QuestId;
}

// Raised each time a kill (or future objective type) advances an objective counter, so the
// tracker can repaint without polling.
public struct QuestObjectiveProgressedEvent : IEvent
{
    public string QuestId;
    public int    StageIndex;
    public int    ObjectiveIndex;
    public int    Current;
    public int    Required;
}

// Raised when a quest moves to its next stage (all current-stage objectives met, more stages left).
public struct QuestStageChangedEvent : IEvent
{
    public string QuestId;
    public int    StageIndex;
}

// Raised once every objective is complete and the quest is waiting to be handed in at its giver.
public struct QuestReadyToTurnInEvent : IEvent
{
    public string QuestId;
}

// Raised when the quest is handed in and finished.
public struct QuestCompletedEvent : IEvent
{
    public string QuestId;
}
