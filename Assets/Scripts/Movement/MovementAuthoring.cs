using Sisus.Init;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MovementAuthoring : MonoBehaviour<MovementSystem>
{
    public NavMeshAgent Agent { get; private set; }
    public MovementType CurrentType { get; private set; }
    public IMovementData CurrentData { get; private set; }

    [SerializeField] private MovementType startingMovement = MovementType.Wander;
    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private float wanderIdleDuration = 2f;

    private MovementSystem movementSystem;

    protected override void Init(MovementSystem movementSystem)
    {
        this.movementSystem = movementSystem;
    }

    protected override void OnAwake()
    {
        Agent = GetComponent<NavMeshAgent>();
    }

    private void OnEnable()
    {
        // Initialize default data based on starting movement
        if (startingMovement == MovementType.Wander)
        {
            SetMovement(new WanderMovementData
            {
                OriginPosition = transform.position,
                WanderRadius = wanderRadius,
                WaitTimer = 0f,
                IdleDuration = wanderIdleDuration,
                IsWaiting = false
            });
        }

        if (movementSystem != null)
        {
            movementSystem.Register(this);
        }
    }

    private void OnDisable()
    {
        if (movementSystem != null)
        {
            movementSystem.Unregister(this);
        }
    }

    public void SetMovement(IMovementData newData)
    {
        CurrentData = newData;
        CurrentType = newData?.Type ?? MovementType.None;
    }
}
