using System.Collections.Generic;
using Sisus.Init;
using UnityEngine;


/// <summary>
/// Thin controller attached to any Unit.
/// Delegates all casting logic to ISpells, and handles VFX via pooling.
/// Manages local cast state (Idle/Casting/OnCooldown) for cast time spells.
/// </summary>
public class Spell : MonoBehaviour<ISpells, ISpellEvents>
{
    public SpellCastState CastState => castState;
    public float CastProgress => currentCastSpell != null && currentCastSpell.CastTime > 0f
        ? 1f - (castTimer / currentCastSpell.CastTime)
        : 0f;

    public void CastSpellBySlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= spells.Count)
            return;

        SpellDataSO spell = spells[slotIndex];
        if (spell == null)
            return;

        TryCast(spell);
    }

    public void InterruptCast()
    {
        if (castState != SpellCastState.Casting) return;

        var spell = currentCastSpell;

        if (spellEvents is SpellsService service)
        {
            service.RaiseCastInterrupted(new SpellCastInterruptedEvent(owner, spell));
        }

        Debug.Log($"[SpellCaster] {owner.name} cast of {spell.SpellName} was interrupted");
        ResetCastState();
    }

    public float GetCooldownRemaining(SpellDataSO spell)
    {
        return spellsSystem.GetCooldownRemaining(owner, spell);
    }

    protected override void Init(ISpells spellsSystem, ISpellEvents spellEvents)
    {
        this[nameof(spellsSystem)] = spellsSystem;
        this[nameof(spellEvents)] = spellEvents;
    }

    private void TryCast(SpellDataSO spell)
    {
        if (castState == SpellCastState.Casting)
        {
            Debug.LogWarning("[SpellCaster] Already casting a spell");
            return;
        }

        if (!spellsSystem.CanCast(owner, spell))
            return;

        Unit target = GetTarget(spell);
        if (target == null)
        {
            Debug.LogWarning($"[SpellCaster] No valid target for {spell.SpellName}");
            return;
        }

        if (!spellsSystem.IsInRange(owner, spell, target))
        {
            Debug.LogWarning($"[SpellCaster] Target out of range for {spell.SpellName}");
            return;
        }

        if (spell.IsInstant)
        {
            ExecuteCast(spell, target);
        }
        else
        {
            StartCasting(spell, target);
        }
    }

    private void StartCasting(SpellDataSO spell, Unit target)
    {
        castState = SpellCastState.Casting;
        currentCastSpell = spell;
        currentCastTarget = target;
        castTimer = spell.CastTime;

        if (spellEvents is SpellsService service)
        {
            service.RaiseCastStarted(new SpellCastStartedEvent(owner, target, spell, spell.CastTime));
        }
    }

    private void UpdateCasting()
    {
        castTimer -= Time.deltaTime;

        if (castTimer <= 0f)
        {
            CompleteCast();
        }
    }

    private void CompleteCast()
    {
        var spell = currentCastSpell;
        var target = currentCastTarget;

        ResetCastState();

        ExecuteCast(spell, target);
    }

    private void Update()
    {
        if (castState == SpellCastState.Casting)
        {
            UpdateCasting();
        }
    }

    protected override void OnAwake()
    {
        owner = GetComponent<Unit>();
    }

    private void ExecuteCast(SpellDataSO spell, Unit target)
    {
        if (!spellsSystem.CastSpell(owner, spell, target))
            return;

        SpawnVFX(spell, target);
    }

    private Unit GetTarget(SpellDataSO spell)
    {
        switch (spell.TargetType)
        {
            case TargetType.Self:
                return owner;

            case TargetType.Enemy:
            case TargetType.Ally:
                return owner.GetCurrentTarget();

            default:
                return null;
        }
    }

    private Transform GetVisualTransform(Unit target)
    {
        return target.GetVisualTransform();
    }

    private void ResetCastState()
    {
        castState = SpellCastState.Idle;
        currentCastSpell = null;
        currentCastTarget = null;
        castTimer = 0f;
    }

    private void SpawnVFX(SpellDataSO spell, Unit target)
    {
        if (!spell.HasVFX) return;

        Transform vfxTransform = GetVisualTransform(target);
        GameObject vfx = vfxPool.Get(spell.ImpactVFXPrefab, vfxTransform.position, Quaternion.identity, vfxTransform);
        vfxPool.Release(spell.ImpactVFXPrefab, vfx, spell.VFXDuration);
    }

    [Header("Spells")]
    [SerializeField] private List<SpellDataSO> spells = new();

    private readonly ISpells spellsSystem = null;
    private readonly ISpellEvents spellEvents = null;
    private readonly SpellVFXPool vfxPool = new();
    private SpellCastState castState = SpellCastState.Idle;
    private SpellDataSO currentCastSpell;
    private Unit currentCastTarget;
    private Unit owner;
    private float castTimer;
}
