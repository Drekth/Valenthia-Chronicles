using System;
using UnityEngine;

// What must happen for a quest stage to advance — never how far we are (progress lives on
// ActiveQuest). Polymorphic and serialized via [SerializeReference] inside QuestStage, so the
// Inspector's managed-reference picker offers every concrete objective without a custom drawer —
// the same pattern as SpellEffect.

[Serializable]
public abstract class QuestObjective
{
    public abstract string Describe();
    public abstract int    RequiredAmount { get; }
}

// Defeat a number of units sharing a target id. The QuestManager matches it against Unit.Id on
// each UnitDiedEvent; the objective itself never subscribes (it lives on the shared, immutable SO).
[Serializable]
public class KillObjective : QuestObjective
{
    public override string Describe()
    {
        return string.IsNullOrEmpty(TargetLabel) ? $"Defeat {TargetId}" : $"Defeat {TargetLabel}";
    }

    public override int RequiredAmount => RequiredCount;

    public bool Matches(string UnitId)
    {
        return !string.IsNullOrEmpty(TargetId) && TargetId == UnitId;
    }

    [SerializeField] private string TargetId;
    [SerializeField] private string TargetLabel;
    [SerializeField] private int    RequiredCount = 1;
}
