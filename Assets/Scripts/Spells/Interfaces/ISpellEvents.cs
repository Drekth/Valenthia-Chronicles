using System;


public interface ISpellEvents
{
    event Action<SpellCastStartedEvent> OnSpellCastStarted;
    event Action<SpellCastEvent> OnSpellCast;
    event Action<SpellCastInterruptedEvent> OnSpellCastInterrupted;
    event Action<SpellHitEvent> OnSpellHit;
    event Action<SpellCooldownEvent> OnCooldownStarted;
    event Action<SpellCooldownEvent> OnCooldownEnded;
}
