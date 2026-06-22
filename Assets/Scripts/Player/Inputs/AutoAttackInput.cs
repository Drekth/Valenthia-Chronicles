using UnityEngine;
using UnityEngine.InputSystem;

// Right-click world interaction (WoW-style). On the right mouse button it raycasts under the cursor:
// if it hits a friendly NPC carrying a DialogueSource it opens that conversation; otherwise it
// toggles the player's auto-attack on the current target. Mirrors ActionBarInput / TargetingInput's
// self-contained style — it owns a code-created InputAction independent of the shared input asset,
// tracks the possessed PlayerCharacter via the spawn channel, and blocks while a loot window or a
// dialogue is open so it never steals the click. Lives on the persistent player rig alongside
// PlayerController and TargetingInput.
public class AutoAttackInput : MonoBehaviour
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
        ToggleAction = new InputAction("AutoAttack", InputActionType.Button, ToggleBinding);
        ToggleAction.performed += HandleToggle;
    }

    private void OnEnable()
    {
        ToggleAction.Enable();

        SpawnedBinding = new EventBinding<PlayerSpawnedEvent>(Possess);
        EventBus<PlayerSpawnedEvent>.Register(SpawnedBinding);

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
        ToggleAction.Disable();

        EventBus<PlayerSpawnedEvent>.Deregister(SpawnedBinding);
        EventBus<ContainerOpenedEvent>.Deregister(ContainerOpenedBinding);
        EventBus<ContainerClosedEvent>.Deregister(ContainerClosedBinding);
        EventBus<DialogueStartedEvent>.Deregister(DialogueStartedBinding);
        EventBus<DialogueEndedEvent>.Deregister(DialogueEndedBinding);
    }

    private void OnDestroy()
    {
        ToggleAction.performed -= HandleToggle;
        ToggleAction.Dispose();
    }

    private void HandleToggle(InputAction.CallbackContext Context)
    {
        if (Blocked || Character == null)
        {
            return;
        }

        // Talk to a friendly NPC under the cursor; otherwise fall back to toggling auto-attack.
        if (TryTalk())
        {
            return;
        }

        Character.ToggleAutoAttack();
    }

    // Raycasts under the pointer; if it hits an NPC with a DialogueSource, selects it and starts its
    // conversation. Returns true when a dialogue opened, so the caller skips the auto-attack toggle.
    private bool TryTalk()
    {
        if (ViewCamera == null || Pointer.current == null)
        {
            return false;
        }

        Vector2 ScreenPosition = Pointer.current.position.ReadValue();
        Ray PointerRay = ViewCamera.ScreenPointToRay(ScreenPosition);

        if (!Physics.Raycast(PointerRay, out RaycastHit Hit, MaxInteractDistance, InteractMask))
        {
            return false;
        }

        DialogueSource Source = Hit.collider.GetComponentInParent<DialogueSource>();
        if (Source == null || Source.Dialogue == null)
        {
            return false;
        }

        if (!ServiceLocator.TryGet<DialogueRunner>(out DialogueRunner Runner))
        {
            return false;
        }

        // Select the NPC so the target halo reflects who we are talking to (WoW-style).
        Unit Npc = Hit.collider.GetComponentInParent<Unit>();
        if (Npc != null && ServiceLocator.TryGet<SelectionManager>(out SelectionManager Selection))
        {
            Selection.Select(Npc);
        }

        Runner.StartDialogue(Source.Dialogue, Source.DisplayName);
        return true;
    }

    // Cache the body the zone announced so the right click toggles the active player's auto-attack.
    private void Possess(PlayerSpawnedEvent Event)
    {
        Character = Event.Character;
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
    [SerializeField] private string ToggleBinding = "<Mouse>/rightButton";

    [Header("Raycast")]
    [SerializeField] private Camera ViewCamera;
    [SerializeField] private LayerMask InteractMask;

    private InputAction ToggleAction;
    private PlayerCharacter Character;
    private bool Blocked;
    private EventBinding<PlayerSpawnedEvent> SpawnedBinding;
    private EventBinding<ContainerOpenedEvent> ContainerOpenedBinding;
    private EventBinding<ContainerClosedEvent> ContainerClosedBinding;
    private EventBinding<DialogueStartedEvent> DialogueStartedBinding;
    private EventBinding<DialogueEndedEvent> DialogueEndedBinding;
}
