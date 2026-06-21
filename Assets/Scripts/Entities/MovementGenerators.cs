using UnityEngine;
using UnityEngine.AI;

// Movement-generator strategy (TrinityCore-inspired). A creature owns one instance of each
// generator; CreatureMotion swaps the active one and the MotionManager ticks it. Each generator
// is a pure "where do I go" policy — deciding WHEN to switch (aggro, leash, attack) is the AI's
// job, not the generator's.
public interface IMovementGenerator
{
    // Called once when this generator becomes the active one for the creature.
    void Begin(CreatureMotion Motion, NavMeshAgent Agent);

    // Called every frame by MotionManager while this generator is active.
    void Tick(CreatureMotion Motion, NavMeshAgent Agent, float DeltaTime);
}

// Passive wander around the spawn point: idle for a random spell, then stroll to a random
// NavMesh point within the wander radius and repeat. Extracted verbatim from the old
// MotionManager loop so existing creatures behave identically.
public class WanderMovement : IMovementGenerator
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public void Begin(CreatureMotion Motion, NavMeshAgent Agent)
    {
        IdleTimer = Random.Range(Motion.Data.MinIdle, Motion.Data.MaxIdle);

        // Walk back toward the spawn point first; the arrival check then drops us into the
        // idle/wander cycle. On the very first Begin (spawn) Origin == position, so this resolves
        // to Idle next tick — matching the old startup behaviour.
        if (Agent.isOnNavMesh)
        {
            Agent.SetDestination(Motion.Origin);
            State = WanderState.Moving;
        }
        else
        {
            State = WanderState.Idle;
        }
    }

    public void Tick(CreatureMotion Motion, NavMeshAgent Agent, float DeltaTime)
    {
        if (State == WanderState.Idle)
        {
            IdleTimer -= DeltaTime;

            if (IdleTimer <= 0.0f)
            {
                if (TryPickWanderTarget(Motion.Origin, Motion.Data.Radius, out Vector3 NewTarget))
                {
                    Agent.SetDestination(NewTarget);
                    State = WanderState.Moving;
                }
                else
                {
                    IdleTimer = Random.Range(Motion.Data.MinIdle, Motion.Data.MaxIdle);
                }
            }
        }
        else
        {
            if (!Agent.pathPending && Agent.remainingDistance <= Agent.stoppingDistance)
            {
                IdleTimer = Random.Range(Motion.Data.MinIdle, Motion.Data.MaxIdle);
                State     = WanderState.Idle;
            }
        }
    }

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private static bool TryPickWanderTarget(Vector3 Origin, float Radius, out Vector3 Result)
    {
        Vector3 RandomPoint = Origin + Random.insideUnitSphere * Radius;
        RandomPoint.y = Origin.y;

        NavMeshHit Hit;
        if (NavMesh.SamplePosition(RandomPoint, out Hit, Radius, NavMesh.AllAreas))
        {
            Result = Hit.position;
            return true;
        }

        Result = Origin;
        return false;
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    private WanderState State;
    private float       IdleTimer;
}

// Pursue a target Transform, re-pathing only when the target has drifted past a small threshold
// to avoid recomputing the path every frame. Stopping at attack range is delegated to the
// NavMeshAgent's stoppingDistance, which the AI sets when it enters combat.
public class ChaseMovement : IMovementGenerator
{
    ////////////////////////////////////////////////////////////
    /// Constants                                            ///
    ////////////////////////////////////////////////////////////

    private const float RepathThresholdSqr = 0.25f; // Re-path once the target moves ~0.5m.

    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public Transform Target;

    public void Begin(CreatureMotion Motion, NavMeshAgent Agent)
    {
        if (Target != null && Agent.isOnNavMesh)
        {
            Agent.SetDestination(Target.position);
            LastDestination = Target.position;
        }
    }

    public void Tick(CreatureMotion Motion, NavMeshAgent Agent, float DeltaTime)
    {
        if (Target == null || !Agent.isOnNavMesh)
        {
            return;
        }

        Vector3 TargetPos = Target.position;
        float DX = TargetPos.x - LastDestination.x;
        float DZ = TargetPos.z - LastDestination.z;

        if ((DX * DX + DZ * DZ) >= RepathThresholdSqr)
        {
            Agent.SetDestination(TargetPos);
            LastDestination = TargetPos;
        }
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    private Vector3 LastDestination;
}
