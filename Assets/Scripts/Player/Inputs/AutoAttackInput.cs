using UnityEngine;
using UnityEngine.InputSystem;

// Auto-attack input: binds the right mouse button to toggle the player's WoW-style auto-attack on
// the current target. Mirrors ActionBarInput / TargetingInput's self-contained style — it owns a
// code-created InputAction independent of the shared input asset, tracks the possessed
// PlayerCharacter via the spawn channel, and blocks while a loot window is open so it never steals
// the click. Lives on the persistent player rig alongside PlayerController and TargetingInput.
public class AutoAttackInput : MonoBehaviour
{
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
    }

    private void OnDisable()
    {
        ToggleAction.Disable();

        EventBus<PlayerSpawnedEvent>.Deregister(SpawnedBinding);
        EventBus<ContainerOpenedEvent>.Deregister(ContainerOpenedBinding);
        EventBus<ContainerClosedEvent>.Deregister(ContainerClosedBinding);
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

        Character.ToggleAutoAttack();
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

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [Header("Input")]
    [SerializeField] private string ToggleBinding = "<Mouse>/rightButton";

    private InputAction ToggleAction;
    private PlayerCharacter Character;
    private bool Blocked;
    private EventBinding<PlayerSpawnedEvent> SpawnedBinding;
    private EventBinding<ContainerOpenedEvent> ContainerOpenedBinding;
    private EventBinding<ContainerClosedEvent> ContainerClosedBinding;
}
