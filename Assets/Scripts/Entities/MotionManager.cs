using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MotionManager : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public void Register(CreatureMotion Motion, NavMeshAgent Agent)
    {
        if (Motion.Data == null)
        {
            Debug.LogWarning($"[MotionManager] {Motion.name} has no CreatureData assigned.");
            return;
        }

        Agent.speed      = Motion.Data.Speed;
        Motion.IdleTimer = Random.Range(Motion.Data.MinIdle, Motion.Data.MaxIdle);
        Motion.State     = WanderState.Idle;
        Creatures.Add((Motion, Agent));
    }

    public void Unregister(CreatureMotion Motion)
    {
        for (int I = Creatures.Count - 1; I >= 0; I--)
        {
            if (Creatures[I].Motion == Motion)
            {
                Creatures.RemoveAt(I);
                return;
            }
        }
    }

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void Awake()
    {
        Creatures = new List<(CreatureMotion Motion, NavMeshAgent Agent)>();
        ServiceLocator.Register<MotionManager>(this);
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<MotionManager>();
    }

    private void Update()
    {
        float DeltaTime = Time.deltaTime;

        for (int I = 0; I < Creatures.Count; I++)
        {
            ProcessCreature(Creatures[I].Motion, Creatures[I].Agent, DeltaTime);
        }
    }

    private void ProcessCreature(CreatureMotion Motion, NavMeshAgent Agent, float DeltaTime)
    {
        if (Motion.State == WanderState.Idle)
        {
            Motion.IdleTimer -= DeltaTime;

            if (Motion.IdleTimer <= 0.0f)
            {
                if (TryPickWanderTarget(Motion.Origin, Motion.Data.Radius, out Vector3 NewTarget))
                {
                    Motion.Target = NewTarget;
                    Agent.SetDestination(NewTarget);
                    Motion.State = WanderState.Moving;
                }
                else
                {
                    Motion.IdleTimer = Random.Range(Motion.Data.MinIdle, Motion.Data.MaxIdle);
                }
            }
        }
        else
        {
            if (!Agent.pathPending && Agent.remainingDistance <= Agent.stoppingDistance)
            {
                Motion.IdleTimer = Random.Range(Motion.Data.MinIdle, Motion.Data.MaxIdle);
                Motion.State     = WanderState.Idle;
            }
        }
    }

    private bool TryPickWanderTarget(Vector3 Origin, float Radius, out Vector3 Result)
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

    private List<(CreatureMotion Motion, NavMeshAgent Agent)> Creatures;
}
