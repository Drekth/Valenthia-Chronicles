using System;
using System.Collections.Generic;
using Sisus.Init;
using UnityEngine;


[Service(typeof(ISpells), typeof(ISpellEvents))]
public sealed class SpellsService : ISpells, ISpellEvents, IUpdate
{
    public event Action<SpellCastStartedEvent> OnSpellCastStarted;
    public event Action<SpellCastEvent> OnSpellCast;
    public event Action<SpellCastInterruptedEvent> OnSpellCastInterrupted;
    public event Action<SpellHitEvent> OnSpellHit;
    public event Action<SpellCooldownEvent> OnCooldownStarted;
    public event Action<SpellCooldownEvent> OnCooldownEnded;

    public SpellsService(IDamageCalculator damageCalculator)
    {
        this.damageCalculator = damageCalculator;
    }

    public bool CanCast(Unit caster, SpellDataSO spell)
    {
        if (caster == null || spell == null)
            return false;

        if (!HasEnoughResource(caster, spell))
            return false;

        if (IsOnCooldown(caster, spell))
            return false;

        return true;
    }

    public bool IsInRange(Unit caster, SpellDataSO spell, Unit target)
    {
        if (spell.TargetType == TargetType.Self)
            return true;

        if (target == null)
            return false;

        float distance = Vector3.Distance(caster.transform.position, target.transform.position);
        return distance <= spell.Range;
    }

    public bool CastSpell(Unit caster, SpellDataSO spell, Unit target)
    {
        if (!CanCast(caster, spell))
        {
            Debug.LogWarning($"[Spells] Cannot cast {spell.SpellName}");
            return false;
        }

        if (target == null)
        {
            Debug.LogWarning($"[Spells] No valid target for {spell.SpellName}");
            return false;
        }

        if (!IsInRange(caster, spell, target))
        {
            Debug.LogWarning($"[Spells] {spell.SpellName} target out of range");
            return false;
        }

        ConsumeResource(caster, spell);

        int damageDealt = ApplyDamage(spell, caster, target);

        StartCooldown(caster, spell);

        OnSpellCast?.Invoke(new SpellCastEvent(caster, target, spell, Time.time));

        if (damageDealt > 0)
        {
            OnSpellHit?.Invoke(new SpellHitEvent(caster, target, spell, damageDealt));
        }

        Debug.Log($"[Spells] {caster.name} cast {spell.SpellName} on {target.name} for {damageDealt} damage");

        return true;
    }

    public float GetCooldownRemaining(Unit caster, SpellDataSO spell)
    {
        return cooldowns.TryGetValue((caster, spell), out float remaining)
            ? Mathf.Max(0f, remaining)
            : 0f;
    }

    public bool IsOnCooldown(Unit caster, SpellDataSO spell)
    {
        return cooldowns.TryGetValue((caster, spell), out float remaining) && remaining > 0f;
    }

    void IUpdate.Update(float deltaTime)
    {
        UpdateCooldowns(deltaTime);
    }

    private void UpdateCooldowns(float deltaTime)
    {
        expiredKeys.Clear();
        activeKeys.Clear();

        // Collect keys into reusable buffer — no per-frame allocation
        foreach (var entry in cooldowns)
        {
            if (entry.Value <= 0f)
                expiredKeys.Add(entry.Key);
            else
                activeKeys.Add(entry.Key);
        }

        // Remove expired
        foreach (var key in expiredKeys)
        {
            cooldowns.Remove(key);
            OnCooldownEnded?.Invoke(new SpellCooldownEvent(key.Item2, key.Item2.Cooldown, 0f));
        }

        // Decrement remaining cooldowns
        foreach (var key in activeKeys)
        {
            cooldowns[key] -= deltaTime;
        }
    }

    private bool HasEnoughResource(Unit caster, SpellDataSO spell)
    {
        return spell.ResourceType switch
        {
            ResourceType.Mana => caster.CurrentMana >= spell.ResourceCost,
            ResourceType.Stamina => caster.Attributes.CurrentStamina >= spell.ResourceCost,
            _ => true
        };
    }

    private void ConsumeResource(Unit caster, SpellDataSO spell)
    {
        if (spell.ResourceCost <= 0) return;

        switch (spell.ResourceType)
        {
            case ResourceType.Mana:
                caster.Attributes.ModifyMana(-spell.ResourceCost);
                break;
            case ResourceType.Stamina:
                caster.Attributes.ModifyStamina(-spell.ResourceCost);
                break;
        }
    }

    private int ApplyDamage(SpellDataSO spell, Unit caster, Unit target)
    {
        var context = new SpellDamageContext(spell, caster, target);
        int damageDealt = damageCalculator.Calculate(context);

        if (damageDealt > 0)
        {
            target.Attributes.ModifyHealth(-damageDealt);
        }

        return damageDealt;
    }

    private void StartCooldown(Unit caster, SpellDataSO spell)
    {
        if (!spell.HasCooldown) return;

        cooldowns[(caster, spell)] = spell.Cooldown;
        OnCooldownStarted?.Invoke(new SpellCooldownEvent(spell, spell.Cooldown, spell.Cooldown));
    }

    // Called by SpellCaster to raise cast time events
    internal void RaiseCastStarted(SpellCastStartedEvent evt) => OnSpellCastStarted?.Invoke(evt);
    internal void RaiseCastInterrupted(SpellCastInterruptedEvent evt) => OnSpellCastInterrupted?.Invoke(evt);

    private readonly IDamageCalculator damageCalculator;
    private readonly Dictionary<(Unit, SpellDataSO), float> cooldowns = new();
    private readonly List<(Unit, SpellDataSO)> expiredKeys = new();
    private readonly List<(Unit, SpellDataSO)> activeKeys = new();
}
