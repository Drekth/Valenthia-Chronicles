using System.Collections;
using UnityEngine;

[RequireComponent(typeof(StatComponent))]
public class Unit : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    // Stable identifier of what this unit is (e.g. "wolf"), used by quest KillObjective matching.
    // Empty for units that are not quest targets.
    public string Id => UnitId;

    public float CurrentHealth => Health;
    public float MaximumHealth => Stats.Get(StatType.MaxHealth);
    public float CurrentMana   => Mana;
    public float MaximumMana   => Stats.Get(StatType.MaxMana);
    public bool IsSelectable      => HasFlag(UnitFlags.IsSelectable);
    public bool IsAttackable      => HasFlag(UnitFlags.IsAttackable);
    public bool IsDead            => HasFlag(UnitFlags.IsDead);
    public bool InCombat          => HasFlag(UnitFlags.InCombat);
    public bool IsMovementDisabled => HasFlag(UnitFlags.MovementDisabled);

    // Reference-counted combat membership (who this unit is fighting). Created in Awake; every unit
    // has one.
    public CombatManager Combat { get; private set; }

    // Threat ledger for targeting. Null for units that cannot hold threat (the player, friendly
    // creatures); attached by CreatureAI for hostile creatures. ApplyDamage feeds it at the source.
    public ThreatManager Threat { get; private set; }

    // Wires a threat ledger onto this unit. Called once by CreatureAI in Awake for hostile creatures.
    public void AttachThreat(ThreatManager Manager)
    {
        Threat = Manager;
    }

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
    // A dead unit absorbs nothing further. A surviving receiver feeds threat to the attacker (if it
    // holds a threat ledger) and establishes a mutual combat reference. A lethal blow ends here — the
    // dead unit holds no threat and joins no combat.
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
            MaxHealth     = MaximumHealth,
        });

        if (Health <= 0.0f)
        {
            Die(Info.Source);
            return;
        }

        if (Info.Source != null)
        {
            if (Threat != null)
            {
                Threat.AddThreat(Info.Source, Info.Amount);
            }

            Combat.SetInCombatWith(Info.Source);
        }
    }

    // Applies a heal: increases health up to max and announces it.
    public void RestoreHealth(float Amount)
    {
        if (IsDead || Amount <= 0.0f)
        {
            return;
        }

        float Actual = Mathf.Min(Amount, MaximumHealth - Health);
        if (Actual <= 0.0f)
        {
            return;
        }

        Health += Actual;

        EventBus<UnitHealthChangedEvent>.Raise(new UnitHealthChangedEvent
        {
            Target        = this,
            CurrentHealth = Health,
            MaxHealth     = MaximumHealth,
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
            MaxMana     = MaximumMana,
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

        float Actual = Mathf.Min(Amount, MaximumMana - Mana);
        if (Actual <= 0.0f)
        {
            return;
        }

        Mana += Actual;

        EventBus<ManaChangedEvent>.Raise(new ManaChangedEvent
        {
            Target      = this,
            CurrentMana = Mana,
            MaxMana     = MaximumMana,
        });
    }

    // Called by CombatManager when this unit gains its first combat reference. Cancels any pending
    // out-of-combat linger and flags the unit in combat.
    public void NotifyEnteredCombat()
    {
        CancelCombatExitTimer();
        SetCombatFlag(true);
    }

    // Called by CombatManager when this unit loses its last combat reference. CombatExitDelay > 0
    // (typically the player) lingers in combat for that delay before dropping; otherwise it drops
    // immediately (AI-driven creatures leave the delay at 0).
    public void NotifyExitedCombat()
    {
        if (CombatExitDelay > 0.0f)
        {
            RestartCombatExitTimer();
        }
        else
        {
            SetCombatFlag(false);
        }
    }

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    // Drives the InCombat flag, raising the state-changed event only on an actual transition so the
    // linger timer and repeated references stay idempotent.
    private void SetCombatFlag(bool InCombatNow)
    {
        if (HasFlag(UnitFlags.InCombat) == InCombatNow)
        {
            return;
        }

        SetFlag(UnitFlags.InCombat, InCombatNow);

        EventBus<UnitCombatStateChangedEvent>.Raise(new UnitCombatStateChangedEvent
        {
            Unit     = this,
            InCombat = InCombatNow,
        });
    }

    private void Awake()
    {
        Stats  = GetComponent<StatComponent>();
        Combat = new CombatManager(this);
    }

    // Current pools start full. Done in Start (not Awake) so a creature's StatComponent has already
    // received its profile from CreatureData (CreatureMotion.Awake) before we read the derived
    // maxima.
    private void Start()
    {
        Health = MaximumHealth;
        Mana   = MaximumMana;

        // Announce starting vitals so UI that bound before Start (e.g. the HUD on
        // PlayerSpawnedEvent, raised in OnEnable) reflects the initial pools instead of the
        // pre-Start zero. Listeners that don't track this unit simply ignore it.
        EventBus<UnitHealthChangedEvent>.Raise(new UnitHealthChangedEvent
        {
            Target        = this,
            CurrentHealth = Health,
            MaxHealth     = MaximumHealth,
        });

        EventBus<ManaChangedEvent>.Raise(new ManaChangedEvent
        {
            Target      = this,
            CurrentMana = Mana,
            MaxMana     = MaximumMana,
        });
    }

    // Marks the unit dead (so it stops being selectable/attackable), exits combat, and announces
    // it once, so selection clearing, loot, and death visuals can react without polling.
    private void Die(Unit Killer)
    {
        Flags |= UnitFlags.IsDead | UnitFlags.MovementDisabled;
        Flags &= ~(UnitFlags.IsSelectable | UnitFlags.IsAttackable);

        // Drop my combat references on every unit I was fighting (cascading their combat-drop) and
        // wipe my threat. Other creatures holding me as an attacker prune me in their own update.
        Combat.EndAllCombat();
        if (Threat != null)
        {
            Threat.ResetThreat();
        }

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
        SetCombatFlag(false);
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [Header("Identity")]
    [SerializeField] private string UnitId;

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
    private StatComponent Stats;
}
