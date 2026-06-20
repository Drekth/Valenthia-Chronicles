using System.Collections.Generic;
using UnityEngine;

// Generic spell-casting brain carried by any entity that can cast (player today, creatures
// later). Owns the rules — target/range/cooldown validation — and the effect resolution, but not
// the visuals: the caller plays the animation, and the impact frame calls NotifyImpact.
//
// Cast pipeline:
//   TryCast  — validate, start cooldown, store the pending cast, raise SpellCastEvent, return.
//   (animation plays)
//   NotifyImpact — resolve the pending cast's effects on the still-valid target, then clear it.
//
// Cooldowns use Time.time (scaled), so they freeze for free when TimeManager pauses the game.
public class SpellCaster : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public SpellCastResult TryCast(SpellData Spell, Unit Target)
    {
        SpellCastResult Result = Validate(Spell, Target);
        if (Result != SpellCastResult.Success)
        {
            EventBus<SpellCastFailedEvent>.Raise(new SpellCastFailedEvent
            {
                Caster = OwnerUnit,
                Spell  = Spell,
                Reason = Result,
            });
            return Result;
        }

        ReadyTime[Spell] = Time.time + Spell.Cooldown;

        PendingSpell   = Spell;
        PendingContext = new SpellCastContext
        {
            Caster          = OwnerUnit,
            CasterTransform = transform,
            Target          = Target,
        };

        EventBus<SpellCastEvent>.Raise(new SpellCastEvent
        {
            Caster = OwnerUnit,
            Spell  = Spell,
            Target = Target,
        });

        return SpellCastResult.Success;
    }

    // Called from the attack animation's impact frame (via SpellAnimationRelay). Resolves the
    // pending cast on a still-living target, then clears it.
    public void NotifyImpact()
    {
        if (PendingSpell == null)
        {
            return;
        }

        Unit Target = PendingContext.Target;
        if (Target != null && !Target.IsDead)
        {
            List<SpellEffect> Effects = PendingSpell.Effects;
            for (int I = 0; I < Effects.Count; I++)
            {
                if (Effects[I] != null)
                {
                    Effects[I].Resolve(in PendingContext);
                }
            }
        }

        PendingSpell = null;
    }

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void Awake()
    {
        OwnerUnit = GetComponent<Unit>();
    }

    private SpellCastResult Validate(SpellData Spell, Unit Target)
    {
        if (Spell == null)
        {
            return SpellCastResult.InvalidSpell;
        }

        if (ReadyTime.TryGetValue(Spell, out float Ready) && Time.time < Ready)
        {
            return SpellCastResult.OnCooldown;
        }

        if (Target == null)
        {
            return SpellCastResult.NoTarget;
        }

        if (Target.IsDead)
        {
            return SpellCastResult.TargetDead;
        }

        if (!InRange(Target, Spell.Range))
        {
            return SpellCastResult.OutOfRange;
        }

        return SpellCastResult.Success;
    }

    // Range check on the ground plane (Y ignored), matching the targeting/selection convention.
    private bool InRange(Unit Target, float Range)
    {
        Vector3 Self = transform.position;
        Vector3 Other = Target.transform.position;
        float DX = Self.x - Other.x;
        float DZ = Self.z - Other.z;
        return (DX * DX + DZ * DZ) <= (Range * Range);
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    private Unit OwnerUnit;
    private SpellData PendingSpell;
    private SpellCastContext PendingContext;
    private readonly Dictionary<SpellData, float> ReadyTime = new Dictionary<SpellData, float>();
}
