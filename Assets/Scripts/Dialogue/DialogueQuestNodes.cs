using System;
using UnityEngine;

// Quest-aware dialogue nodes. They carry only a QuestData reference; the DialogueRunner interprets
// them against the IQuestManager (offerable? ready to hand in?) and routes through the two outputs,
// the same way ConditionNode branches True/False. Discovered automatically by the GraphView editor
// (reflection over DialogueNode subtypes), so no editor change is needed to author them.

// Offers a quest to the player. Output 0 (Accepted) is taken after the player accepts and the quest
// is started; output 1 (Declined) is taken on refusal — and also auto-taken, without showing the
// offer, when the quest cannot be offered (already active or completed).
[Serializable]
public class QuestOfferNode : DialogueNode
{
    public override string DisplayName => "Quest Offer";

    public QuestData Quest => OfferedQuest;

    public override int    OutputCount               => 2;
    public override string GetOutputLabel(int Index)  => Index == 0 ? "Accepted" : "Declined";
    public override string GetOutputTarget(int Index) => Index == 0 ? AcceptedGuid : DeclinedGuid;

    public override void SetOutputTarget(int Index, string TargetGuid)
    {
        if (Index == 0)
        {
            AcceptedGuid = TargetGuid;
        }
        else
        {
            DeclinedGuid = TargetGuid;
        }
    }

    public override string Summary => OfferedQuest != null ? OfferedQuest.Title : "(no quest assigned)";

    [SerializeField] private QuestData OfferedQuest;

    [SerializeField, HideInInspector] private string AcceptedGuid;
    [SerializeField, HideInInspector] private string DeclinedGuid;
}

// Hands a quest in at its giver. Output 0 (Completed) is taken after the player confirms the
// hand-in; output 1 (NotReady) is taken otherwise — including when the player has not finished the
// objectives, is not on the quest, or already turned it in.
[Serializable]
public class QuestTurnInNode : DialogueNode
{
    public override string DisplayName => "Quest Turn-In";

    public QuestData Quest => TurnInQuest;

    public override int    OutputCount               => 2;
    public override string GetOutputLabel(int Index)  => Index == 0 ? "Completed" : "NotReady";
    public override string GetOutputTarget(int Index) => Index == 0 ? CompletedGuid : NotReadyGuid;

    public override void SetOutputTarget(int Index, string TargetGuid)
    {
        if (Index == 0)
        {
            CompletedGuid = TargetGuid;
        }
        else
        {
            NotReadyGuid = TargetGuid;
        }
    }

    public override string Summary => TurnInQuest != null ? TurnInQuest.Title : "(no quest assigned)";

    [SerializeField] private QuestData TurnInQuest;

    [SerializeField, HideInInspector] private string CompletedGuid;
    [SerializeField, HideInInspector] private string NotReadyGuid;
}
