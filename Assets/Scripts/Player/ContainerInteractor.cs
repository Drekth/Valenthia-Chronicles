using UnityEngine;
using UnityEngine.InputSystem;

// Turns a mouse click into a world interaction: on the click action it casts a ray from
// the camera through the pointer and opens any Container it hits on the interactable layer.
// Owns a self-contained InputAction (bound to the configured pointer button) so world
// interaction stays independent of the shared input asset. Event-driven (no Update);
// blocked while a loot window is open to avoid click-through. Lives on the persistent
// player rig alongside the PlayerController.
public class ContainerInteractor : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Constants                                            ///
    ////////////////////////////////////////////////////////////

    private const float MaxInteractDistance = 1000.0f;

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void Awake()
    {
        SelectAction = new InputAction("Select", InputActionType.Button, ClickBinding);
        SelectAction.performed += HandleSelect;
    }

    private void OnEnable()
    {
        if (SelectAction != null)
        {
            SelectAction.Enable();
        }

        ContainerOpenedBinding = new EventBinding<ContainerOpenedEvent>(HandleContainerOpened);
        EventBus<ContainerOpenedEvent>.Register(ContainerOpenedBinding);

        ContainerClosedBinding = new EventBinding<ContainerClosedEvent>(HandleContainerClosed);
        EventBus<ContainerClosedEvent>.Register(ContainerClosedBinding);

        DialogueStartedBinding = new EventBinding<DialogueStartedEvent>(HandleDialogueStarted);
        EventBus<DialogueStartedEvent>.Register(DialogueStartedBinding);

        DialogueEndedBinding = new EventBinding<DialogueEndedEvent>(HandleDialogueEnded);
        EventBus<DialogueEndedEvent>.Register(DialogueEndedBinding);
    }

    private void OnDisable()
    {
        if (SelectAction != null)
        {
            SelectAction.Disable();
        }

        EventBus<ContainerOpenedEvent>.Deregister(ContainerOpenedBinding);
        EventBus<ContainerClosedEvent>.Deregister(ContainerClosedBinding);
        EventBus<DialogueStartedEvent>.Deregister(DialogueStartedBinding);
        EventBus<DialogueEndedEvent>.Deregister(DialogueEndedBinding);
    }

    private void OnDestroy()
    {
        if (SelectAction != null)
        {
            SelectAction.performed -= HandleSelect;
            SelectAction.Dispose();
        }
    }

    private void HandleSelect(InputAction.CallbackContext Context)
    {
        if (Blocked || ViewCamera == null || Pointer.current == null)
        {
            return;
        }

        Vector2 ScreenPosition = Pointer.current.position.ReadValue();
        Ray PointerRay = ViewCamera.ScreenPointToRay(ScreenPosition);

        if (!Physics.Raycast(PointerRay, out RaycastHit Hit, MaxInteractDistance, InteractableMask))
        {
            return;
        }

        Container Target = Hit.collider.GetComponentInParent<Container>();
        if (Target != null)
        {
            Target.Open();
        }
    }

    private void HandleContainerOpened(ContainerOpenedEvent Event)
    {
        Blocked = true;
    }

    private void HandleContainerClosed()
    {
        Blocked = false;
    }

    private void HandleDialogueStarted(DialogueStartedEvent Event)
    {
        Blocked = true;
    }

    private void HandleDialogueEnded()
    {
        Blocked = false;
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [Header("Input")]
    [SerializeField] private string ClickBinding = "<Mouse>/leftButton";

    [Header("Raycast")]
    [SerializeField] private Camera ViewCamera;
    [SerializeField] private LayerMask InteractableMask;

    private InputAction SelectAction;
    private bool Blocked;
    private EventBinding<ContainerOpenedEvent> ContainerOpenedBinding;
    private EventBinding<ContainerClosedEvent> ContainerClosedBinding;
    private EventBinding<DialogueStartedEvent> DialogueStartedBinding;
    private EventBinding<DialogueEndedEvent> DialogueEndedBinding;
}
