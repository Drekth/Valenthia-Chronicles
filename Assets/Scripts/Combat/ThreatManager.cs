using System.Collections.Generic;
using UnityEngine;

// A single attacker's accumulated threat against a creature.
public struct ThreatEntry
{
    public Unit  Attacker;
    public float Threat;
}

// Pure threat data for one creature: how much threat each attacker has accumulated, plus the taunt
// and fixate overrides. Holds no selection logic (that is the ITargetSelector's job) and no Unity
// dependency beyond the Unit reference, so it can later bake into an ECS buffer.
public class ThreatTable
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public List<ThreatEntry> Entries => ThreatEntries;

    public Unit  TauntTarget;
    public float TauntEndTime;
    public Unit  FixateTarget;

    // Accumulates threat for an attacker (creating its entry on first contact) and returns the new
    // total. Callers are expected to have validated the attacker.
    public float AddThreat(Unit Attacker, float Amount)
    {
        for (int I = 0; I < ThreatEntries.Count; I++)
        {
            if (ThreatEntries[I].Attacker == Attacker)
            {
                ThreatEntry Updated = ThreatEntries[I];
                Updated.Threat   += Amount;
                ThreatEntries[I]  = Updated;
                return Updated.Threat;
            }
        }

        ThreatEntries.Add(new ThreatEntry { Attacker = Attacker, Threat = Amount });
        return Amount;
    }

    public float GetThreat(Unit Attacker)
    {
        for (int I = 0; I < ThreatEntries.Count; I++)
        {
            if (ThreatEntries[I].Attacker == Attacker)
            {
                return ThreatEntries[I].Threat;
            }
        }

        return 0.0f;
    }

    public float HighestThreat()
    {
        float Max = 0.0f;
        for (int I = 0; I < ThreatEntries.Count; I++)
        {
            if (ThreatEntries[I].Threat > Max)
            {
                Max = ThreatEntries[I].Threat;
            }
        }
        return Max;
    }

    public void Remove(Unit Attacker)
    {
        for (int I = ThreatEntries.Count - 1; I >= 0; I--)
        {
            if (ThreatEntries[I].Attacker == Attacker)
            {
                ThreatEntries.RemoveAt(I);
                return;
            }
        }
    }

    public void Clear()
    {
        ThreatEntries.Clear();
        TauntTarget  = null;
        TauntEndTime = 0.0f;
        FixateTarget = null;
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    private readonly List<ThreatEntry> ThreatEntries = new List<ThreatEntry>();
}

// Per-creature threat orchestration (TrinityCore-inspired). Owns the threat data (ThreatTable) and a
// pluggable victim selector. Plain C# owned by a creature's Unit and ticked from CreatureAI (which
// MotionManager already drives), so it adds no Update loop of its own. Resolves the fixate and taunt
// overrides itself, leaving the selector to reason purely about threat numbers.
public class ThreatManager
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public Unit CurrentVictim => Victim;

    public ThreatManager(Unit Owner, ITargetSelector Selector)
    {
        this.Owner    = Owner;
        this.Selector = Selector;
        this.Table    = new ThreatTable();
    }

    // Adds threat from an attacker (damage feed or an ability). Ignores invalid attackers so the
    // table never holds the owner itself, the dead, or the unattackable.
    public void AddThreat(Unit Attacker, float Amount)
    {
        if (!IsValidAttacker(Attacker))
        {
            return;
        }

        float NewThreat = Table.AddThreat(Attacker, Amount);

        EventBus<ThreatChangedEvent>.Raise(new ThreatChangedEvent
        {
            Creature  = Owner,
            Attacker  = Attacker,
            NewThreat = NewThreat,
        });
    }

    // Forces the taunter to be the victim for a duration, and lifts its threat to the current top so
    // it stays a credible victim once the taunt expires (TrinityCore behaviour).
    public void Taunt(Unit Taunter, float Duration)
    {
        if (!IsValidAttacker(Taunter))
        {
            return;
        }

        float Highest = Table.HighestThreat();
        float Current = Table.GetThreat(Taunter);
        if (Current < Highest)
        {
            Table.AddThreat(Taunter, Highest - Current);
        }

        Table.TauntTarget  = Taunter;
        Table.TauntEndTime = Time.time + Duration;
    }

    // Forces a specific target regardless of threat until cleared.
    public void Fixate(Unit Target)
    {
        if (!IsValidAttacker(Target))
        {
            return;
        }

        Table.FixateTarget = Target;
    }

    public void ClearFixate()
    {
        Table.FixateTarget = null;
    }

    // Wipes all threat and overrides (used on leash/evade and on the owner's death).
    public void ResetThreat()
    {
        Table.Clear();
        SetVictim(null);
    }

    // Per-frame maintenance: expire the taunt, drop invalid attackers, then reselect the victim.
    public void Update(float DeltaTime)
    {
        if (Table.TauntTarget != null && Time.time >= Table.TauntEndTime)
        {
            Table.TauntTarget  = null;
            Table.TauntEndTime = 0.0f;
        }

        PruneInvalid();

        SetVictim(ResolveVictim());
    }

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    // Fixate wins over taunt, which wins over raw threat. A stale override (its target died or
    // became unattackable) is cleared here so selection falls back to the threat table.
    private Unit ResolveVictim()
    {
        if (Table.FixateTarget != null)
        {
            if (IsValidAttacker(Table.FixateTarget))
            {
                return Table.FixateTarget;
            }
            Table.FixateTarget = null;
        }

        if (Table.TauntTarget != null)
        {
            if (IsValidAttacker(Table.TauntTarget))
            {
                return Table.TauntTarget;
            }
            Table.TauntTarget  = null;
            Table.TauntEndTime = 0.0f;
        }

        return Selector.Select(Table, Victim);
    }

    private void PruneInvalid()
    {
        for (int I = Table.Entries.Count - 1; I >= 0; I--)
        {
            if (!IsValidAttacker(Table.Entries[I].Attacker))
            {
                Table.Entries.RemoveAt(I);
            }
        }
    }

    private void SetVictim(Unit NewVictim)
    {
        if (Victim == NewVictim)
        {
            return;
        }

        Unit Old = Victim;
        Victim    = NewVictim;

        EventBus<VictimChangedEvent>.Raise(new VictimChangedEvent
        {
            Creature  = Owner,
            OldVictim = Old,
            NewVictim = NewVictim,
        });
    }

    private bool IsValidAttacker(Unit Attacker)
    {
        return Attacker != null
            && Attacker != Owner
            && !Attacker.IsDead
            && Attacker.IsAttackable;
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    private readonly Unit            Owner;
    private readonly ITargetSelector Selector;
    private readonly ThreatTable     Table;

    private Unit Victim;
}
