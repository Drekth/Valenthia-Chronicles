using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CreatureMotion : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public CreatureData Data => CreatureDataAsset;

    // ECS-like state: written directly by MotionManager
    public Vector3     Origin;
    public Vector3     Target;
    public float       IdleTimer;
    public WanderState State;

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void Awake()
    {
        NavAgent = GetComponent<NavMeshAgent>();
        Origin   = transform.position;
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

    private NavMeshAgent NavAgent;
}
