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

        Agent.speed = Motion.Data.Speed;
        Motion.CurrentGenerator.Begin(Motion, Agent);

        CreatureAI AI = Motion.GetComponent<CreatureAI>();
        Creatures.Add((Motion, Agent, AI));
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
        Creatures = new List<(CreatureMotion Motion, NavMeshAgent Agent, CreatureAI AI)>();
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
            CreatureMotion Motion = Creatures[I].Motion;
            NavMeshAgent   Agent  = Creatures[I].Agent;
            CreatureAI     AI     = Creatures[I].AI;

            // The brain decides (and may swap the active generator) before the generator moves.
            if (AI != null)
            {
                AI.Tick(DeltaTime);
            }

            Motion.CurrentGenerator.Tick(Motion, Agent, DeltaTime);
        }
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    private List<(CreatureMotion Motion, NavMeshAgent Agent, CreatureAI AI)> Creatures;
}
