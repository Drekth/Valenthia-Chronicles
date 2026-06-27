using System.Collections.Generic;

// Reference-counted combat membership for a Unit (TrinityCore-inspired CombatManager). A unit is
// "in combat" while it holds at least one combat reference to another unit. References are mutual:
// engaging a target also makes the target engage back. Plain C# owned by Unit; PvE only.
//
// The owner drives the actual UnitFlags.InCombat flag and the optional out-of-combat linger via
// NotifyEnteredCombat / NotifyExitedCombat — this manager only tracks the reference set.
public class CombatManager
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public bool IsInCombat => References.Count > 0;

    public CombatManager(Unit Owner)
    {
        this.Owner = Owner;
    }

    // Engage another unit, establishing a mutual combat reference. Safe to call repeatedly.
    public void SetInCombatWith(Unit Other)
    {
        if (Other == null || Other == Owner || Other.IsDead || Owner.IsDead)
        {
            return;
        }

        AddReference(Other);

        if (Other.Combat != null)
        {
            Other.Combat.AddReference(Owner);
        }
    }

    // Drop the mutual combat reference with a single unit.
    public void EndCombatWith(Unit Other)
    {
        if (Other == null)
        {
            return;
        }

        RemoveReference(Other);

        if (Other.Combat != null)
        {
            Other.Combat.RemoveReference(Owner);
        }
    }

    // Drop every combat reference (on both sides) — used on death, leash, or evade.
    public void EndAllCombat()
    {
        for (int I = References.Count - 1; I >= 0; I--)
        {
            Unit Other = References[I];
            if (Other != null && Other.Combat != null)
            {
                Other.Combat.RemoveReference(Owner);
            }
        }

        References.Clear();
        Owner.NotifyExitedCombat();
    }

    // One-directional reference add; the mutual side is handled by SetInCombatWith.
    public void AddReference(Unit Other)
    {
        if (References.Contains(Other))
        {
            return;
        }

        References.Add(Other);

        if (References.Count == 1)
        {
            Owner.NotifyEnteredCombat();
        }
    }

    // One-directional reference removal; the mutual side is handled by EndCombatWith.
    public void RemoveReference(Unit Other)
    {
        if (!References.Remove(Other))
        {
            return;
        }

        if (References.Count == 0)
        {
            Owner.NotifyExitedCombat();
        }
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    private readonly Unit       Owner;
    private readonly List<Unit> References = new List<Unit>();
}
