using System;

// Bitmask of runtime states/capabilities carried by a Unit (WoW-style unit flags).
// Meant to grow: a single Unit can be selectable, attackable, dead, in combat, etc.
[Flags]
public enum UnitFlags
{
    None         = 0,
    IsSelectable = 1 << 0,
    IsDead       = 1 << 1,
    IsAttackable = 1 << 2,
    InCombat     = 1 << 3,
}
