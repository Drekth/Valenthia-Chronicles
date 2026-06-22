using System;
using System.Collections.Generic;
using UnityEngine;

// Immutable definition of a quest, authored as a DT_ asset. Progress is never stored here — it
// lives on the runtime ActiveQuest (see QuestManager). Mirrors the SpellData/DialogueData split
// between a shared definition and per-instance state.

public enum QuestStatus
{
    None,
    Active,
    ReadyToTurnIn,
    Completed,
}

// One reward line shown on the turn-in panel. v1 only displays it; granting items is wired later
// when an inventory grant API exists.
[Serializable]
public class QuestReward
{
    public string Describe()
    {
        if (Item != null)
        {
            return Count > 1 ? $"{Item.DisplayName} x{Count}" : Item.DisplayName;
        }
        if (Gold > 0)
        {
            return $"{Gold} gold";
        }
        if (Xp > 0)
        {
            return $"{Xp} XP";
        }
        return string.Empty;
    }

    [SerializeField] private ItemData Item;
    [SerializeField] private int      Count = 1;
    [SerializeField] private int      Gold;
    [SerializeField] private int      Xp;
}

// One step of a quest: journal text plus the objectives that must all complete to advance. The
// objectives are polymorphic ([SerializeReference]), same pattern as SpellEffect.
[Serializable]
public class QuestStage
{
    public string                        Description => StageDescription;
    public IReadOnlyList<QuestObjective> Objectives  => StageObjectives;

    [SerializeField]     private string               StageDescription;
    [SerializeReference] private List<QuestObjective> StageObjectives = new List<QuestObjective>();
}

[CreateAssetMenu(menuName = "Valenthia/Narrative/Quest")]
public class QuestData : ScriptableObject
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public string                     Id      => QuestId;
    public string                     Title   => QuestTitle;
    public string                     Summary => QuestSummary;
    public IReadOnlyList<QuestStage>  Stages  => QuestStages;
    public IReadOnlyList<QuestReward> Rewards => QuestRewards;

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [Header("Identity")]
    [SerializeField] private string QuestId;
    [SerializeField] private string QuestTitle;
    [TextArea]
    [SerializeField] private string QuestSummary;

    [Header("Flow")]
    [SerializeField] private List<QuestStage> QuestStages = new List<QuestStage>();

    [Header("Rewards")]
    [SerializeField] private List<QuestReward> QuestRewards = new List<QuestReward>();
}
