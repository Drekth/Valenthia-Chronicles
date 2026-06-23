using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CreatureMotion : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public CreatureData Data => CreatureDataAsset;

    // Unit on this same GameObject; may be null if the creature has no Unit component.
    public Unit Owner { get; private set; }

    // Spawn anchor: wander samples around it and the AI leashes back to it.
    public Vector3 Origin;

    // Active movement policy; the MotionManager ticks it, the AI swaps it.
    public IMovementGenerator CurrentGenerator { get; private set; }

    // Switch to chasing a target, re-initialising the chase generator from the current position.
    public void SwitchToChase(Transform Target)
    {
        Chase.Target     = Target;
        CurrentGenerator = Chase;
        CurrentGenerator.Begin(this, NavAgent);
    }

    // Return to passive wandering around the spawn point.
    public void SwitchToWander()
    {
        CurrentGenerator = Wander;
        CurrentGenerator.Begin(this, NavAgent);
    }

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void Awake()
    {
        NavAgent = GetComponent<NavMeshAgent>();
        Owner    = GetComponent<Unit>();
        Origin   = transform.position;

        Wander           = new WanderMovement();
        Chase            = new ChaseMovement();
        CurrentGenerator = Wander;

        ApplyStatProfile();
        SpawnVisual();
    }

    private void Start()
    {
        // Start fires after all Awakes — MotionManager is guaranteed registered
        if (ServiceLocator.TryGet<MotionManager>(out MotionManager Manager))
        {
            Manager.Register(this, NavAgent);
        }
    }

    private void OnDisable()
    {
        if (ServiceLocator.TryGet<MotionManager>(out MotionManager Manager))
        {
            Manager.Unregister(this);
        }
    }

    // Routes the creature's authored stat profile (carried by its CreatureData) into the StatComponent,
    // so every creature stat has a single source. Runs in Awake — before Unit.Start reads the
    // derived maxima. A missing profile is left to the StatComponent (Get returns 0) and warned here.
    private void ApplyStatProfile()
    {
        if (CreatureDataAsset == null)
        {
            return;
        }

        if (CreatureDataAsset.Stats == null)
        {
            Debug.LogWarning($"[CreatureMotion] {name} has no StatProfile on its CreatureData.", this);
            return;
        }

        StatComponent Sheet = GetComponent<StatComponent>();
        if (Sheet != null)
        {
            Sheet.SetData(CreatureDataAsset.Stats);
        }
    }

    private void SpawnVisual()
    {
        if (CreatureDataAsset == null || CreatureDataAsset.Model == null)
        {
            return;
        }

        Transform VisualRoot = transform.Find("VisualRoot");

        if (VisualRoot != null)
        {
            Instantiate(CreatureDataAsset.Model, VisualRoot);
        }
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [SerializeField] private CreatureData CreatureDataAsset;

    private NavMeshAgent   NavAgent;
    private WanderMovement Wander;
    private ChaseMovement  Chase;
}
