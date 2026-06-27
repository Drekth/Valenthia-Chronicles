// Victim-selection strategy for a creature's threat table (TrinityCore-inspired). The ThreatManager
// owns the threat data; an ITargetSelector decides WHICH attacker becomes the victim from that data.
// Taunt and fixate overrides are resolved by the ThreatManager above the selector, so a selector
// only ever reasons about raw threat numbers — keeping it swappable (e.g. a future utility/scoring
// selector) without touching the data model.
public interface ITargetSelector
{
    // Returns the attacker that should be the current victim, or null when the table is empty.
    // CurrentVictim is the previously selected victim, passed so a selector can apply hysteresis.
    Unit Select(ThreatTable Table, Unit CurrentVictim);
}

// Default selector: highest threat wins, but the current victim is only displaced once a contender
// exceeds it by a hysteresis margin. This stops the victim from flapping between two attackers whose
// threat is nearly equal (mirrors TrinityCore's melee threat threshold).
public class HighestThreatSelector : ITargetSelector
{
    ////////////////////////////////////////////////////////////
    /// Constants                                            ///
    ////////////////////////////////////////////////////////////

    // A contender must exceed the current victim's threat by this factor to steal aggro.
    private const float SwitchThreshold = 1.1f;

    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public Unit Select(ThreatTable Table, Unit CurrentVictim)
    {
        Unit  Best       = null;
        float BestThreat = float.NegativeInfinity;

        for (int I = 0; I < Table.Entries.Count; I++)
        {
            ThreatEntry Entry = Table.Entries[I];

            if (Best == null || Entry.Threat > BestThreat)
            {
                BestThreat = Entry.Threat;
                Best       = Entry.Attacker;
            }
        }

        if (Best == null)
        {
            return null;
        }

        // Keep the current victim unless the best contender clears the hysteresis margin.
        if (CurrentVictim != null && Best != CurrentVictim)
        {
            float CurrentThreat = Table.GetThreat(CurrentVictim);
            if (BestThreat < CurrentThreat * SwitchThreshold)
            {
                return CurrentVictim;
            }
        }

        return Best;
    }
}
