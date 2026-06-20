using UnityEngine;

// Single source of truth for the player's current target unit. ServiceLocator-registered
// service living on the persistent player rig. An entity is targetable when its Unit carries
// the IsSelectable flag. Tab cycling discovers candidates on the fly with a physics overlap on
// the Selectable layer, so no per-entity registration component is needed. Target changes are
// broadcast through an SO channel so the halo visual reacts without referencing this logic.
public class SelectionManager : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Constants                                            ///
    ////////////////////////////////////////////////////////////

    private const int MaxCycleCandidates = 64;

    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public Unit CurrentTarget { get; private set; }

    public void Select(Unit Target)
    {
        if (Target == null || !Target.IsSelectable || Target == CurrentTarget)
        {
            return;
        }

        CurrentTarget = Target;
        Broadcast();
    }

    public void Clear()
    {
        if (CurrentTarget == null)
        {
            return;
        }

        CurrentTarget = null;
        Broadcast();
    }

    // Cycle to the next-nearest selectable unit beyond the current one, wrapping back to the
    // globally nearest. Candidates are gathered within CycleRadius on the Selectable layer.
    // Distances are measured on the ground plane (Y ignored).
    public void CycleNearest(Vector3 FromPosition)
    {
        int Count = Physics.OverlapSphereNonAlloc(FromPosition, CycleRadius, CandidateBuffer, SelectableMask);
        if (Count == 0)
        {
            return;
        }

        float CurrentDistance = -1.0f;
        if (CurrentTarget != null)
        {
            CurrentDistance = FlatDistanceSqr(FromPosition, CurrentTarget.transform.position);
        }

        Unit Next = null;
        float NextDistance = float.MaxValue;
        Unit Nearest = null;
        float NearestDistance = float.MaxValue;

        for (int I = 0; I < Count; I++)
        {
            Unit Candidate = CandidateBuffer[I].GetComponentInParent<Unit>();
            if (Candidate == null || !Candidate.IsSelectable)
            {
                continue;
            }

            float Distance = FlatDistanceSqr(FromPosition, Candidate.transform.position);

            // Track the globally nearest as the wrap-around fallback.
            if (Distance < NearestDistance)
            {
                NearestDistance = Distance;
                Nearest = Candidate;
            }

            // Track the closest candidate strictly farther than the current target.
            if (Candidate != CurrentTarget && Distance > CurrentDistance && Distance < NextDistance)
            {
                NextDistance = Distance;
                Next = Candidate;
            }
        }

        if (Next != null)
        {
            Select(Next);
        }
        else if (Nearest != null)
        {
            Select(Nearest);
        }
    }

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void Awake()
    {
        CandidateBuffer = new Collider[MaxCycleCandidates];
        ServiceLocator.Register<SelectionManager>(this);
    }

    private void OnEnable()
    {
        UnitDiedBinding = new EventBinding<UnitDiedEvent>(HandleUnitDied);
        EventBus<UnitDiedEvent>.Register(UnitDiedBinding);
    }

    private void OnDisable()
    {
        EventBus<UnitDiedEvent>.Deregister(UnitDiedBinding);
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<SelectionManager>();
    }

    // Drop the target the moment it dies so the player can't keep attacking a corpse.
    private void HandleUnitDied(UnitDiedEvent Event)
    {
        if (Event.Unit == CurrentTarget)
        {
            Clear();
        }
    }

    private void Broadcast()
    {
        EventBus<TargetChangedEvent>.Raise(new TargetChangedEvent { Target = CurrentTarget });
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

    [Header("Cycling")]
    [SerializeField] private LayerMask SelectableMask;
    [SerializeField] private float CycleRadius = 30.0f;

    private Collider[] CandidateBuffer;
    private EventBinding<UnitDiedEvent> UnitDiedBinding;
}
