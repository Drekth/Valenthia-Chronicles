using UnityEngine;
using UnityEngine.AI;

//////////////////////////////////////////////////////
///            Wander Movement Data                ///
//////////////////////////////////////////////////////
///
public class WanderMovementData : IMovementData
{
    public MovementType Type => MovementType.Wander;

    public Vector3 OriginPosition;
    public float WanderRadius;
    public float WaitTimer;
    public float IdleDuration;
    public bool IsWaiting;
}

//////////////////////////////////////////////////////
///          Wander Movement Processor             ///
//////////////////////////////////////////////////////
///
public class WanderProcessor : IMovementProcessor
{
    public MovementType HandledType => MovementType.Wander;

    public void Process(MovementAuthoring entity, float deltaTime)
    {
        var data = entity.CurrentData as WanderMovementData;
        if (data == null || entity.Agent == null) return;

        if (data.IsWaiting)
        {
            data.WaitTimer -= deltaTime;
            if (data.WaitTimer <= 0f)
            {
                data.IsWaiting = false;
                SetRandomDestination(entity.Agent, data);
            }
        }
        else
        {
            // Check if reached destination
            if (!entity.Agent.pathPending && entity.Agent.remainingDistance <= entity.Agent.stoppingDistance)
            {
                if (!entity.Agent.hasPath || entity.Agent.velocity.sqrMagnitude == 0f)
                {
                    data.IsWaiting = true;
                    data.WaitTimer = data.IdleDuration;
                }
            }
        }
    }

    private void SetRandomDestination(NavMeshAgent agent, WanderMovementData data)
    {
        Vector3 randomDirection = Random.insideUnitSphere * data.WanderRadius;
        randomDirection += data.OriginPosition;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, data.WanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
}
