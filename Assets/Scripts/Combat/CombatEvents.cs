// Events du domaine combat/sorts, publiés/consommés via EventBus<T>.

// Raised by Unit.ApplyDamage after health is reduced, so feedback consumers (floating combat
// text, health bars, threat) react without referencing the damage source.
public struct DamageTakenEvent : IEvent
{
    public Unit Target;
    public Unit Source;
    public float Amount;
    public DamageType School;
}

// Raised once when a Unit's health reaches zero. Killer is the damage source (may be null).
public struct UnitDiedEvent : IEvent
{
    public Unit Unit;
    public Unit Killer;
}

// Raised by a creature brain when its combat phase toggles, so feedback consumers (floating
// health bars, music, threat) react without polling AI state. InCombat is true on entering
// combat, false on leaving.
public struct UnitCombatStateChangedEvent : IEvent
{
    public Unit Unit;
    public bool InCombat;
}

// Raised when a cast is accepted (validated, cooldown started, pending its impact).
public struct SpellCastEvent : IEvent
{
    public Unit Caster;
    public SpellData Spell;
    public Unit Target;
}

// Raised when a cast is rejected, carrying the reason for UI/audio feedback.
public struct SpellCastFailedEvent : IEvent
{
    public Unit Caster;
    public SpellData Spell;
    public SpellCastResult Reason;
}

// Raised by Unit any time health changes (damage, heal, regen). Carries the new absolute values
// so consumers never need to re-query the unit — a single subscription covers every source.
public struct UnitHealthChangedEvent : IEvent
{
    public Unit Target;
    public float CurrentHealth;
    public float MaxHealth;
}

// Raised by Unit any time mana changes (spell cost, regen). Same convention as UnitHealthChangedEvent.
public struct ManaChangedEvent : IEvent
{
    public Unit Target;
    public float CurrentMana;
    public float MaxMana;
}
