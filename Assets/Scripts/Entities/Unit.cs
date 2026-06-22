using UnityEngine;

public class Unit : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    // Stable identifier of what this unit is (e.g. "wolf"), used by quest KillObjective matching.
    // Empty for units that are not quest targets.
    public string Id => UnitId;

    public float CurrentHealth => Health;
    public float MaximumHealth => MaxHealth;
    public bool IsSelectable => HasFlag(UnitFlags.IsSelectable);
    public bool IsAttackable => HasFlag(UnitFlags.IsAttackable);
    public bool IsDead => HasFlag(UnitFlags.IsDead);

    public bool HasFlag(UnitFlags Flag)
    {
        return (Flags & Flag) != 0;
    }

    // Toggles a runtime flag. Used to drive disposition-based state (e.g. a friendly creature is
    // not attackable) without exposing the raw bitmask.
    public void SetFlag(UnitFlags Flag, bool Enable)
    {
        if (Enable)
        {
            Flags |= Flag;
        }
        else
        {
            Flags &= ~Flag;
        }
    }

    // Applies a resolved damage packet: reduces health, announces it, and handles death once.
    // A dead unit absorbs nothing further.
    public void ApplyDamage(in DamageInfo Info)
    {
        if (IsDead)
        {
            return;
        }

        Health = Mathf.Max(0.0f, Health - Info.Amount);

        EventBus<DamageTakenEvent>.Raise(new DamageTakenEvent
        {
            Target = this,
            Source = Info.Source,
            Amount = Info.Amount,
            School = Info.School,
        });

        if (Health <= 0.0f)
        {
            Die(Info.Source);
        }
    }

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void Awake()
    {
        Health = MaxHealth;
    }

    // Marks the unit dead (so it stops being selectable/attackable) and announces it once, so
    // selection clearing, loot, and death visuals can react without polling.
    private void Die(Unit Killer)
    {
        Flags |= UnitFlags.IsDead;
        Flags &= ~(UnitFlags.IsSelectable | UnitFlags.IsAttackable);

        EventBus<UnitDiedEvent>.Raise(new UnitDiedEvent
        {
            Unit   = this,
            Killer = Killer,
        });
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [Header("Identity")]
    [SerializeField] private string UnitId;

    [Header("Health")]
    [SerializeField] private float MaxHealth = 100.0f;

    [Header("Flags")]
    [SerializeField] private UnitFlags Flags = UnitFlags.None;

    private float Health;
}