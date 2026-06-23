using UnityEngine;

// Shared combat/spell definitions: damage schools, cast outcomes, and the lightweight value
// structs that flow through the cast pipeline. WoW-inspired but kept minimal — grows alongside
// the spell effect classes.

// Damage school of an effect (WoW-style). Physical is the default melee school; the magic
// schools are reserved for future spells (mitigation/resistance hooks will branch on this).
public enum DamageType
{
    Physical,
    Fire,
    Frost,
    Nature,
    Shadow,
    Arcane,
    Holy,
}

// Result of a TryCast attempt. Success means the cast was accepted and is now pending its
// impact; every other value is a rejection reason carried by SpellCastFailedEvent.
public enum SpellCastResult
{
    Success,
    NoTarget,
    TargetDead,
    NotAttackable,
    OutOfRange,
    OnCooldown,
    InvalidSpell,
    NotEnoughMana,
}

// Everything an effect needs to apply itself: who casts, from where (for range/facing), and on
// whom. Passed by 'in' so effects read it without copying.
public struct SpellCastContext
{
    public Unit Caster;
    public Transform CasterTransform;
    public Unit Target;
}

// A single resolved damage packet handed to Unit.ApplyDamage. Source may be null when the
// caster has no Unit (kept nullable on purpose for now).
public struct DamageInfo
{
    public Unit Source;
    public Unit Target;
    public float Amount;
    public DamageType School;
}
