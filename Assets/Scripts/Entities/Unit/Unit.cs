using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public interface ISelectable
{
    void OnSelected();
    void OnDeselected();
    bool IsSelectable();
}

/// <summary>
/// Core Unit class — any living entity in the game world (player, creature, NPC).
/// Manages states, flags, stats reference, targeting, and selection.
/// Designed for Valenthia's entity architecture in Unity.
/// </summary>
[RequireComponent(typeof(UnitAttributes))]
public abstract class Unit : WorldObject, ISelectable
{
    //////////////////////////////////////////////////////
    ///               Core Unit Class                  ///
    //////////////////////////////////////////////////////

    protected virtual void Awake()
    {
        attributes = GetComponent<UnitAttributes>();

        if (selectionDecal != null)
            selectionDecal.enabled = false;
    }
    protected virtual void Start()
    {
        if (attributes == null)
        {
            Debug.LogError($"Unit '{gameObject.name}' is missing UnitAttributes component!", this);
            Destroy(gameObject);
        }
    }

    public override TypeId ObjectTypeId => TypeId.Unit;

    //////////////////////////////////////////////////////
    ///              Attributes Accessors              ///
    //////////////////////////////////////////////////////

    public UnitAttributes Attributes => attributes;
    public int CurrentHealth => attributes != null ? attributes.CurrentHealth : 0;
    public int CurrentMana   => attributes != null ? attributes.CurrentMana : 0;
    public int MaxHealth     => attributes != null ? attributes.MaxHealth : 0;
    public int MaxMana       => attributes != null ? attributes.MaxMana : 0;

    // ─── State Properties (composite checks) ────────────────────────────

    public bool IsAlive      => attributes != null && attributes.IsAlive();
    public bool IsDead       => !IsAlive;
    public bool IsInCombat   => HasFlag(UnitFlags.InCombat);
    public bool CanMove      => !HasState(UnitStates.CannotMove) && !HasFlag(UnitFlags.Stunned);
    public bool CanCast      => !HasState(UnitStates.Stunned | UnitStates.Died) && !HasFlag(UnitFlags.Pacified | UnitFlags.Confused);
    public bool CanAttack    => !HasState(UnitStates.Died) && !HasFlag(UnitFlags.Pacified | UnitFlags.Disarmed);
    public bool CanTurn      => !HasState(UnitStates.CannotTurn) && !HasFlag2(UnitFlags2.CannotTurn);

    // ─── Unit State Management (bitflag system) ─────────────────────────

    /// <summary>
    /// Adds one or more states to the unit.
    /// </summary>
    public void AddState(UnitStates states)
    {
        unitStates |= states;
    }

    /// <summary>
    /// Removes one or more states from the unit.
    /// </summary>
    public void RemoveState(UnitStates states)
    {
        unitStates &= ~states;
    }

    /// <summary>
    /// Checks if the unit has ALL of the specified states.
    /// </summary>
    public bool HasState(UnitStates states)
    {
        return (unitStates & states) == states;
    }

    /// <summary>
    /// Checks if the unit has ANY of the specified states.
    /// </summary>
    public bool HasAnyState(UnitStates states)
    {
        return (unitStates & states) != 0;
    }

    /// <summary>
    /// Returns the raw unit state bitmask.
    /// </summary>
    public UnitStates GetUnitState() => unitStates;

    // ─── Unit Flags Management ──────────────────────────────────────────

    public void SetFlag(UnitFlags flag) => unitFlags = unitFlags.With(flag);
    public void RemoveFlag(UnitFlags flag) => unitFlags = unitFlags.Without(flag);
    public bool HasFlag(UnitFlags flag) => unitFlags.Has(flag);

    public void SetFlag2(UnitFlags2 flag) => unitFlags2 = unitFlags2.With(flag);
    public void RemoveFlag2(UnitFlags2 flag) => unitFlags2 = unitFlags2.Without(flag);
    public bool HasFlag2(UnitFlags2 flag) => unitFlags2.Has(flag);

    // ─── Combat State Shortcuts ─────────────────────────────────────────

    public void SetInCombat() => SetFlag(UnitFlags.InCombat);
    public void ClearInCombat() => RemoveFlag(UnitFlags.InCombat);

    // ─── Target Management (existing) ───────────────────────────────────

    public Unit GetCurrentTarget() => currentTarget;

    public void SetCurrentTarget(Unit target) => currentTarget = target;

    // ─── ISelectable (existing) ─────────────────────────────────────────

    public virtual void OnSelected()
    {
        if (selectionDecal != null)
            selectionDecal.enabled = true;
    }

    public virtual void OnDeselected()
    {
        if (selectionDecal != null)
            selectionDecal.enabled = false;
    }

    public virtual bool IsSelectable() => true;

    public virtual Transform GetVisualTransform() => transform;


    // SerializeField
    [Header("Selection")]
    [SerializeField] private DecalProjector selectionDecal;

    // Private fields
    private Unit currentTarget;
    protected UnitAttributes attributes;
    private UnitStates unitStates;
    private UnitFlags unitFlags;
    private UnitFlags2 unitFlags2;

}

/// <summary>
/// Extension methods for UnitFlags manipulation.
/// </summary>
public static class UnitFlagsExtensions
{
    public static bool Has(this UnitFlags self, UnitFlags flag) => (self & flag) != 0;
    public static UnitFlags With(this UnitFlags self, UnitFlags flag) => self | flag;
    public static UnitFlags Without(this UnitFlags self, UnitFlags flag) => self & ~flag;

    public static bool Has(this UnitFlags2 self, UnitFlags2 flag) => (self & flag) != 0;
    public static UnitFlags2 With(this UnitFlags2 self, UnitFlags2 flag) => self | flag;
    public static UnitFlags2 Without(this UnitFlags2 self, UnitFlags2 flag) => self & ~flag;
}
