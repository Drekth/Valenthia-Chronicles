using System.Collections;
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
    public float CurrentMana   => Mana;
    public float MaximumMana   => MaxMana;
    public bool IsSelectable      => HasFlag(UnitFlags.IsSelectable);
    public bool IsAttackable      => HasFlag(UnitFlags.IsAttackable);
    public bool IsDead            => HasFlag(UnitFlags.IsDead);
    public bool InCombat          => HasFlag(UnitFlags.InCombat);
    public bool IsMovementDisabled => HasFlag(UnitFlags.MovementDisabled);

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
    // A dead unit absorbs nothing further. Both receiver and attacker enter combat.
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

        EventBus<UnitHealthChangedEvent>.Raise(new UnitHealthChangedEvent
        {
            Target        = this,
            CurrentHealth = Health,
            MaxHealth     = MaxHealth,
        });

        EnterCombat();

        if (Info.Source != null)
        {
            Info.Source.EnterCombat();
        }

        if (Health <= 0.0f)
        {
            Die(Info.Source);
        }
    }

    // Applies a heal: increases health up to max and announces it.
    public void RestoreHealth(float Amount)
    {
        if (IsDead || Amount <= 0.0f)
        {
            return;
        }

        float Actual = Mathf.Min(Amount, MaxHealth - Health);
        if (Actual <= 0.0f)
        {
            return;
        }

        Health += Actual;

        EventBus<UnitHealthChangedEvent>.Raise(new UnitHealthChangedEvent
        {
            Target        = this,
            CurrentHealth = Health,
            MaxHealth     = MaxHealth,
        });
    }

    // Deducts mana; returns false (no effect) when the pool is insufficient.
    public bool SpendMana(float Amount)
    {
        if (Amount > Mana)
        {
            return false;
        }

        Mana -= Amount;

        EventBus<ManaChangedEvent>.Raise(new ManaChangedEvent
        {
            Target      = this,
            CurrentMana = Mana,
            MaxMana     = MaxMana,
        });

        return true;
    }

    // Replenishes mana up to max and announces it.
    public void RestoreMana(float Amount)
    {
        if (IsDead || Amount <= 0.0f)
        {
            return;
        }

        float Actual = Mathf.Min(Amount, MaxMana - Mana);
        if (Actual <= 0.0f)
        {
            return;
        }

        Mana += Actual;

        EventBus<ManaChangedEvent>.Raise(new ManaChangedEvent
        {
            Target      = this,
            CurrentMana = Mana,
            MaxMana     = MaxMana,
        });
    }

    // Transitions this unit into combat. Safe to call when already in combat — only resets the
    // auto-exit timer. CombatExitDelay <= 0 disables auto-exit (used for creature units whose AI
    // controls the combat state directly).
    public void EnterCombat()
    {
        if (HasFlag(UnitFlags.InCombat))
        {
            RestartCombatExitTimer();
            return;
        }

        SetFlag(UnitFlags.InCombat, true);

        EventBus<UnitCombatStateChangedEvent>.Raise(new UnitCombatStateChangedEvent
        {
            Unit     = this,
            InCombat = true,
        });

        RestartCombatExitTimer();
    }

    // Exits combat. Called externally by AI brains (CreatureAI) or internally by the auto-exit
    // timer. Cancels the timer if it was running.
    public void ExitCombat()
    {
        CancelCombatExitTimer();

        if (!HasFlag(UnitFlags.InCombat))
        {
            return;
        }

        SetFlag(UnitFlags.InCombat, false);

        EventBus<UnitCombatStateChangedEvent>.Raise(new UnitCombatStateChangedEvent
        {
            Unit     = this,
            InCombat = false,
        });
    }

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void Awake()
    {
        Health = MaxHealth;
        Mana   = MaxMana;
    }

    // Marks the unit dead (so it stops being selectable/attackable), exits combat, and announces
    // it once, so selection clearing, loot, and death visuals can react without polling.
    private void Die(Unit Killer)
    {
        Flags |= UnitFlags.IsDead | UnitFlags.MovementDisabled;
        Flags &= ~(UnitFlags.IsSelectable | UnitFlags.IsAttackable);

        ExitCombat();

        EventBus<UnitDiedEvent>.Raise(new UnitDiedEvent
        {
            Unit   = this,
            Killer = Killer,
        });
    }

    private void RestartCombatExitTimer()
    {
        if (CombatExitDelay <= 0.0f)
        {
            return;
        }

        CancelCombatExitTimer();
        CombatExitRoutine = StartCoroutine(CombatExitTimerRoutine());
    }

    private void CancelCombatExitTimer()
    {
        if (CombatExitRoutine != null)
        {
            StopCoroutine(CombatExitRoutine);
            CombatExitRoutine = null;
        }
    }

    private IEnumerator CombatExitTimerRoutine()
    {
        yield return new WaitForSeconds(CombatExitDelay);
        CombatExitRoutine = null;
        ExitCombat();
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [Header("Identity")]
    [SerializeField] private string UnitId;

    [Header("Health")]
    [SerializeField] private float MaxHealth = 100.0f;

    [Header("Mana")]
    [SerializeField] private float MaxMana = 100.0f;

    [Header("Combat")]
    // Auto-exit delay after the last combat action (damage dealt or received). 0 = disabled;
    // AI-driven creatures leave this at 0 so their brain controls combat state directly.
    // Player units typically set this to 6 seconds.
    [SerializeField] private float CombatExitDelay = 0.0f;

    [Header("Flags")]
    [SerializeField] private UnitFlags Flags = UnitFlags.None;

    private float Health;
    private float Mana;
    private Coroutine CombatExitRoutine;
}
