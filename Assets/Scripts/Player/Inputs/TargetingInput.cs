using UnityEngine;
using UnityEngine.InputSystem;

// Turns mouse and keyboard input into target selection: left click raycasts from the camera
// through the pointer and selects any Selectable it hits (clicking empty space clears the
// target), Escape clears, and Tab cycles to the next-nearest unit. Owns self-contained
// InputActions like ContainerInteractor so it stays independent of the shared input asset.
// Tracks the possessed PlayerCharacter (via the spawn channel) to supply the cycle origin,
// and blocks while a loot window is open to avoid stealing the click. Lives on the persistent
// player rig alongside PlayerController and ContainerInteractor.
public class TargetingInput : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Constants                                            ///
    ////////////////////////////////////////////////////////////

    private const float MaxSelectDistance = 1000.0f;

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void Awake()
    {
        ClickAction    = new InputAction("Select",   InputActionType.Button, ClickBinding);
        DeselectAction = new InputAction("Deselect", InputActionType.Button, DeselectBinding);
        CycleAction    = new InputAction("Cycle",    InputActionType.Button, CycleBinding);

        ClickAction.performed    += HandleClick;
        DeselectAction.performed += HandleDeselect;
        CycleAction.performed    += HandleCycle;
    }

    private void OnEnable()
    {
        ClickAction.Enable();
        DeselectAction.Enable();
        CycleAction.Enable();

        if (OnSpawned != null)
        {
            OnSpawned.Subscribe(Possess);
        }

        if (OnContainerOpened != null)
        {
            OnContainerOpened.Subscribe(HandleContainerOpened);
        }

        if (OnContainerClosed != null)
        {
            OnContainerClosed.Subscribe(HandleContainerClosed);
        }
    }

    private void OnDisable()
    {
        ClickAction.Disable();
        DeselectAction.Disable();
        CycleAction.Disable();

        if (OnSpawned != null)
        {
            OnSpawned.Unsubscribe(Possess);
        }

        if (OnContainerOpened != null)
        {
            OnContainerOpened.Unsubscribe(HandleContainerOpened);
        }

        if (OnContainerClosed != null)
        {
            OnContainerClosed.Unsubscribe(HandleContainerClosed);
        }
    }

    private void OnDestroy()
    {
        ClickAction.performed    -= HandleClick;
        DeselectAction.performed -= HandleDeselect;
        CycleAction.performed    -= HandleCycle;

        ClickAction.Dispose();
        DeselectAction.Dispose();
        CycleAction.Dispose();
    }

    private void HandleClick(InputAction.CallbackContext Context)
    {
        if (Blocked || ViewCamera == null || Pointer.current == null)
        {
            return;
        }

        if (!ServiceLocator.TryGet<SelectionManager>(out SelectionManager Manager))
        {
            return;
        }

        Vector2 ScreenPosition = Pointer.current.position.ReadValue();
        Ray PointerRay = ViewCamera.ScreenPointToRay(ScreenPosition);

        if (!Physics.Raycast(PointerRay, out RaycastHit Hit, MaxSelectDistance, SelectableMask))
        {
            Manager.Clear();
            return;
        }

        Unit Target = Hit.collider.GetComponentInParent<Unit>();
        if (Target != null && Target.IsSelectable)
        {
            Manager.Select(Target);
        }
        else
        {
            Manager.Clear();
        }
    }

    private void HandleDeselect(InputAction.CallbackContext Context)
    {
        if (ServiceLocator.TryGet<SelectionManager>(out SelectionManager Manager))
        {
            Manager.Clear();
        }
    }

    private void HandleCycle(InputAction.CallbackContext Context)
    {
        if (Blocked)
        {
            return;
        }

        if (ServiceLocator.TryGet<SelectionManager>(out SelectionManager Manager))
        {
            Manager.CycleNearest(PlayerPosition);
        }
    }

    // Cache the body the zone announced so Tab cycling measures distance from the player.
    private void Possess(PlayerCharacter NewCharacter)
    {
        Character = NewCharacter;
    }

    private void HandleContainerOpened(Container Opened)
    {
        Blocked = true;
    }

    private void HandleContainerClosed()
    {
        Blocked = false;
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [Header("Input")]
    [SerializeField] private string ClickBinding    = "<Mouse>/leftButton";
    [SerializeField] private string DeselectBinding = "<Keyboard>/escape";
    [SerializeField] private string CycleBinding    = "<Keyboard>/tab";

    [Header("Raycast")]
    [SerializeField] private Camera ViewCamera;
    [SerializeField] private LayerMask SelectableMask;

    [Header("Events")]
    [SerializeField] private PlayerCharacterEventChannel OnSpawned;
    [SerializeField] private ContainerEventChannel OnContainerOpened;
    [SerializeField] private VoidEventChannel OnContainerClosed;

    private Vector3 PlayerPosition => Character != null ? Character.transform.position : transform.position;

    private InputAction ClickAction;
    private InputAction DeselectAction;
    private InputAction CycleAction;
    private PlayerCharacter Character;
    private bool Blocked;
}
