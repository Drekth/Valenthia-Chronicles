using System;
using UnityEngine;

// Polymorphic spell effects. A SpellData holds a [SerializeReference] list of these, each
// applying itself in Resolve — no central switch, so adding a new effect (heal, aura, ...) is a
// new class, never an edit to the cast pipeline. v1 ships a single DamageEffect.

[Serializable]
public abstract class SpellEffect
{
    public abstract void Resolve(in SpellCastContext Context);
}

// Deals flat damage to the cast target. The flat BaseAmount is the extension point for future
// scaling (coefficient * caster spell power) — that logic lands here, not in the pipeline.
[Serializable]
public class DamageEffect : SpellEffect
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public override void Resolve(in SpellCastContext Context)
    {
        if (Context.Target == null)
        {
            return;
        }

        DamageInfo Info = new DamageInfo
        {
            Source = Context.Caster,
            Target = Context.Target,
            Amount = BaseAmount,
            School = School,
        };

        Context.Target.ApplyDamage(Info);
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [SerializeField] private float BaseAmount = 15.0f;
    [SerializeField] private DamageType School = DamageType.Physical;
}

// Deals physical damage read from the caster's equipped MainHand weapon — the auto-attack effect.
// Independent of any spell's flat amount: the value comes from the weapon (ItemData.AttackDamage),
// falling back to UnarmedDamage when no weapon is worn. Today only the player owns an Equipment, so
// the weapon is resolved through the ServiceLocator; creature auto-attacks keep their own damage.
[Serializable]
public class WeaponDamageEffect : SpellEffect
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public override void Resolve(in SpellCastContext Context)
    {
        if (Context.Target == null)
        {
            return;
        }

        float Amount = UnarmedDamage;
        if (ServiceLocator.TryGet<Equipment>(out Equipment Gear))
        {
            ItemData Weapon = Gear.Get(EquipmentSlot.MainHand);
            if (Weapon != null)
            {
                Amount = Weapon.AttackDamage;
            }
        }

        DamageInfo Info = new DamageInfo
        {
            Source = Context.Caster,
            Target = Context.Target,
            Amount = Amount,
            School = DamageType.Physical,
        };

        Context.Target.ApplyDamage(Info);
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [SerializeField] private float UnarmedDamage = 2.0f;
}
