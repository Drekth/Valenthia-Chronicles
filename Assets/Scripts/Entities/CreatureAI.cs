using UnityEngine;
using UnityEngine.AI;

// Reactive brain carried by a creature alongside its Unit / CreatureMotion / NavMeshAgent. It
// decides WHEN to fight and drives the movement generators accordingly; CreatureMotion owns the
// "how to move". Disposition (ReactState) comes from the creature's CreatureData:
//   Friendly   — not attackable, never reacts.
//   Neutral    — retaliates only once damaged (defensive).
//   Aggressive — attacks the player on sight within AggroRadius.
//
// The MotionManager ticks this brain each frame (before the active movement generator). Aggro
// scanning is throttled and uses a shared NonAlloc overlap buffer to stay off the GC.
[RequireComponent(typeof(CreatureMotion))]
[RequireComponent(typeof(Unit))]
public class CreatureAI : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Constants                                            ///
    ////////////////////////////////////////////////////////////

    private const int   MaxAggroCandidates = 16;
    private const float AggroScanInterval  = 0.2f;

    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    // Per-frame decision pass, driven by MotionManager so there is a single creature update loop.
    public void Tick(float DeltaTime)
    {
        if (OwnerUnit.IsDead || Motion.Data == null)
        {
            return;
        }

        if (State == CombatState.Idle)
        {
            TickIdle();
        }
        else
        {
            TickCombat();
        }
    }

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void Awake()
    {
        Motion    = GetComponent<CreatureMotion>();
        NavAgent  = GetComponent<NavMeshAgent>();
        OwnerUnit = GetComponent<Unit>();

        DefaultStoppingDistance = NavAgent.stoppingDistance;

        // Disposition gates attackability: a friendly creature simply lacks the IsAttackable flag,
        // so SpellCaster.Validate rejects the player's attacks on it.
        if (Motion.Data != null)
        {
            OwnerUnit.SetFlag(UnitFlags.IsAttackable, Motion.Data.Reaction != ReactState.Friendly);
        }
    }

    private void OnEnable()
    {
        DamageTakenBinding = new EventBinding<DamageTakenEvent>(HandleDamageTaken);
        EventBus<DamageTakenEvent>.Register(DamageTakenBinding);
    }

    private void OnDisable()
    {
        EventBus<DamageTakenEvent>.Deregister(DamageTakenBinding);
    }

    private void TickIdle()
    {
        // Only aggressive creatures hunt proactively; neutrals wait to be hit (see HandleDamageTaken).
        if (Motion.Data.Reaction != ReactState.Aggressive)
        {
            return;
        }

        if (Time.time < NextAggroScanTime)
        {
            return;
        }
        NextAggroScanTime = Time.time + AggroScanInterval;

        Unit Found = ScanForTarget();
        if (Found != null)
        {
            EnterCombat(Found);
        }
    }

    private void TickCombat()
    {
        // Give up if the target is gone, dead, or we have been dragged too far from home.
        if (CurrentTarget == null || CurrentTarget.IsDead || IsLeashed())
        {
            ExitCombat();
            return;
        }

        if (InAttackRange(CurrentTarget) && Time.time >= NextAttackTime)
        {
            PerformAttack();
            NextAttackTime = Time.time + Motion.Data.AttackCooldown;
        }
    }

    // Defensive retaliation: any non-friendly creature that takes damage latches onto its attacker.
    private void HandleDamageTaken(DamageTakenEvent Event)
    {
        if (Event.Target != OwnerUnit || State == CombatState.Combat || Motion.Data == null)
        {
            return;
        }

        if (Motion.Data.Reaction == ReactState.Friendly)
        {
            return;
        }

        if (Event.Source == null || Event.Source.IsDead)
        {
            return;
        }

        EnterCombat(Event.Source);
    }

    private void EnterCombat(Unit Target)
    {
        CurrentTarget          = Target;
        State                  = CombatState.Combat;
        NavAgent.stoppingDistance = Motion.Data.AttackRange;
        Motion.SwitchToChase(Target.transform);

        EventBus<UnitCombatStateChangedEvent>.Raise(new UnitCombatStateChangedEvent
        {
            Unit     = OwnerUnit,
            InCombat = true,
        });
    }

    private void ExitCombat()
    {
        CurrentTarget            = null;
        State                    = CombatState.Idle;
        NavAgent.stoppingDistance = DefaultStoppingDistance;
        NextAggroScanTime        = Time.time + AggroScanInterval;
        Motion.SwitchToWander();

        EventBus<UnitCombatStateChangedEvent>.Raise(new UnitCombatStateChangedEvent
        {
            Unit     = OwnerUnit,
            InCombat = false,
        });
    }

    private void PerformAttack()
    {
        FaceTarget();

        DamageInfo Info = new DamageInfo
        {
            Source = OwnerUnit,
            Target = CurrentTarget,
            Amount = Motion.Data.AttackDamage,
            School = DamageType.Physical,
        };
        CurrentTarget.ApplyDamage(in Info);
    }

    private Unit ScanForTarget()
    {
        int Count = Physics.OverlapSphereNonAlloc(transform.position, Motion.Data.AggroRadius, AggroBuffer, PlayerMask);

        for (int I = 0; I < Count; I++)
        {
            Unit Candidate = AggroBuffer[I].GetComponentInParent<Unit>();
            if (Candidate != null && !Candidate.IsDead && Candidate.IsAttackable)
            {
                return Candidate;
            }
        }

        return null;
    }

    // Leash on the creature's own distance from its spawn point, so it cannot be kited forever.
    private bool IsLeashed()
    {
        float Leash = Motion.Data.LeashRadius;
        return FlatDistanceSqr(transform.position, Motion.Origin) > (Leash * Leash);
    }

    private bool InAttackRange(Unit Target)
    {
        float Range = Motion.Data.AttackRange;
        return FlatDistanceSqr(transform.position, Target.transform.position) <= (Range * Range);
    }

    private void FaceTarget()
    {
        Vector3 Direction = CurrentTarget.transform.position - transform.position;
        Direction.y = 0.0f;

        if (Direction.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(Direction);
        }
    }

    private static float FlatDistanceSqr(Vector3 A, Vector3 B)
    {
        float DX = A.x - B.x;
        float DZ = A.z - B.z;
        return DX * DX + DZ * DZ;
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [Header("Aggro")]
    [SerializeField] private LayerMask PlayerMask;

    private CreatureMotion Motion;
    private NavMeshAgent   NavAgent;
    private Unit           OwnerUnit;

    private CombatState State;
    private Unit        CurrentTarget;
    private float       NextAttackTime;
    private float       NextAggroScanTime;
    private float       DefaultStoppingDistance;

    private EventBinding<DamageTakenEvent> DamageTakenBinding;

    private static readonly Collider[] AggroBuffer = new Collider[MaxAggroCandidates];
}
