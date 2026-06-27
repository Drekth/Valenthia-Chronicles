using UnityEngine;
using UnityEngine.AI;

// Reactive brain carried by a creature alongside its Unit / CreatureMotion / NavMeshAgent. It
// decides WHEN to fight and drives the movement generators accordingly; CreatureMotion owns the
// "how to move". Disposition (ReactState) comes from the creature's CreatureData:
//   Friendly   — not attackable, holds no threat ledger, never reacts.
//   Neutral    — retaliates only once damaged (threat is fed at the source by Unit.ApplyDamage).
//   Aggressive — proactively seeds threat on players it spots within AggroRadius.
//
// Targeting is delegated to the owner's ThreatManager: the brain reads CurrentVictim each tick and
// chases/attacks it, rather than tracking its own target. The MotionManager ticks this brain each
// frame (before the active movement generator). Aggro scanning is throttled and uses a shared
// NonAlloc overlap buffer to stay off the GC.
[RequireComponent(typeof(CreatureMotion))]
[RequireComponent(typeof(Unit))]
public class CreatureAI : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Constants                                            ///
    ////////////////////////////////////////////////////////////

    private const int   MaxAggroCandidates = 16;
    private const float AggroScanInterval  = 0.2f;

    // Threat granted to a player spotted by a proactive aggro scan, so it becomes the victim before
    // dealing any damage. Kept small so real damage quickly dominates the table.
    private const float AggroSeedThreat = 1.0f;

    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    // Per-frame decision pass, driven by MotionManager so there is a single creature update loop.
    public void Tick(float DeltaTime)
    {
        if (OwnerUnit.IsDead || Motion.Data == null || Threat == null)
        {
            return;
        }

        // Refresh the threat table (expire taunt, prune the dead, reselect) before reading the victim.
        Threat.Update(DeltaTime);

        Unit Victim = Threat.CurrentVictim;

        if (Victim == null)
        {
            if (State == CombatState.Combat)
            {
                ExitCombat();
            }
            else
            {
                TickIdle();
            }
            return;
        }

        // Give up if we have been dragged too far from home; drop threat and return.
        if (IsLeashed())
        {
            ExitCombat();
            return;
        }

        if (State == CombatState.Idle || Victim != CurrentTarget)
        {
            EnterCombat(Victim);
        }

        TickCombat(Victim);
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

        if (Motion.Data == null)
        {
            return;
        }

        // Disposition gates attackability: a friendly creature simply lacks the IsAttackable flag,
        // so SpellCaster.Validate rejects the player's attacks on it. Only hostile creatures hold a
        // threat ledger — friendly ones keep Threat null and therefore never fight.
        bool Hostile = Motion.Data.Reaction != ReactState.Friendly;
        OwnerUnit.SetFlag(UnitFlags.IsAttackable, Hostile);

        if (Hostile)
        {
            Threat = new ThreatManager(OwnerUnit, new HighestThreatSelector());
            OwnerUnit.AttachThreat(Threat);
        }
    }

    private void TickIdle()
    {
        // Only aggressive creatures hunt proactively; neutrals wait until damage feeds their threat.
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
            // Seed threat; next tick's Threat.Update promotes it to victim and drops us into combat.
            Threat.AddThreat(Found, AggroSeedThreat);
        }
    }

    private void TickCombat(Unit Victim)
    {
        if (InAttackRange(Victim) && Time.time >= NextAttackTime)
        {
            PerformAttack(Victim);
            NextAttackTime = Time.time + Motion.Data.AttackCooldown;
        }
    }

    private void EnterCombat(Unit Target)
    {
        CurrentTarget             = Target;
        State                     = CombatState.Combat;
        NavAgent.stoppingDistance = Motion.Data.AttackRange;
        Motion.SwitchToChase(Target.transform);

        OwnerUnit.Combat.SetInCombatWith(Target);
    }

    private void ExitCombat()
    {
        CurrentTarget             = null;
        State                     = CombatState.Idle;
        NavAgent.stoppingDistance = DefaultStoppingDistance;
        NextAggroScanTime         = Time.time + AggroScanInterval;
        Motion.SwitchToWander();

        Threat.ResetThreat();
        OwnerUnit.Combat.EndAllCombat();
    }

    private void PerformAttack(Unit Victim)
    {
        FaceTarget(Victim);

        DamageInfo Info = new DamageInfo
        {
            Source = OwnerUnit,
            Target = Victim,
            Amount = Motion.Data.AttackDamage,
            School = DamageType.Physical,
        };
        Victim.ApplyDamage(in Info);
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

    private void FaceTarget(Unit Target)
    {
        Vector3 Direction = Target.transform.position - transform.position;
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
    private ThreatManager  Threat;

    private CombatState State;
    private Unit        CurrentTarget;
    private float       NextAttackTime;
    private float       NextAggroScanTime;
    private float       DefaultStoppingDistance;

    private static readonly Collider[] AggroBuffer = new Collider[MaxAggroCandidates];
}
