using UnityEngine;
using UnityEngine.InputSystem;

// Action-bar input: binds the number-row keys (1..9, 0) to the player's action-bar slots and casts
// the bound spell on press. Mirrors TargetingInput's self-contained style — it owns code-created
// InputActions independent of the shared input asset, tracks the possessed PlayerCharacter via the
// spawn channel, and blocks while a loot window is open. Lives on the persistent player rig
// alongside PlayerController and TargetingInput.
public class ActionBarInput : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void Awake()
    {
        SlotActions = new InputAction[SlotBindings.Length];
        for (int I = 0; I < SlotBindings.Length; I++)
        {
            int Slot = I;
            InputAction Action = new InputAction("Slot" + (Slot + 1), InputActionType.Button, SlotBindings[Slot]);
            Action.performed += Context => HandleSlot(Slot);
            SlotActions[Slot] = Action;
        }
    }

    private void OnEnable()
    {
        for (int I = 0; I < SlotActions.Length; I++)
        {
            SlotActions[I].Enable();
        }

        SpawnedBinding = new EventBinding<PlayerSpawnedEvent>(Possess);
        EventBus<PlayerSpawnedEvent>.Register(SpawnedBinding);

        ContainerOpenedBinding = new EventBinding<ContainerOpenedEvent>(HandleContainerOpened);
        EventBus<ContainerOpenedEvent>.Register(ContainerOpenedBinding);

        ContainerClosedBinding = new EventBinding<ContainerClosedEvent>(HandleContainerClosed);
        EventBus<ContainerClosedEvent>.Register(ContainerClosedBinding);
    }

    private void OnDisable()
    {
        for (int I = 0; I < SlotActions.Length; I++)
        {
            SlotActions[I].Disable();
        }

        EventBus<PlayerSpawnedEvent>.Deregister(SpawnedBinding);
        EventBus<ContainerOpenedEvent>.Deregister(ContainerOpenedBinding);
        EventBus<ContainerClosedEvent>.Deregister(ContainerClosedBinding);
    }

    private void OnDestroy()
    {
        for (int I = 0; I < SlotActions.Length; I++)
        {
            SlotActions[I].Dispose();
        }
    }

    private void HandleSlot(int Slot)
    {
        if (Blocked || Character == null)
        {
            return;
        }

        Character.CastSlot(Slot);
    }

    // Cache the body the zone announced so action-bar keys cast from the active player.
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
    [SerializeField] private string[] SlotBindings = new string[]
    {
        "<Keyboard>/1", "<Keyboard>/2", "<Keyboard>/3", "<Keyboard>/4", "<Keyboard>/5",
        "<Keyboard>/6", "<Keyboard>/7", "<Keyboard>/8", "<Keyboard>/9", "<Keyboard>/0",
    };

    private InputAction[] SlotActions;
    private PlayerCharacter Character;
    private bool Blocked;
    private EventBinding<PlayerSpawnedEvent> SpawnedBinding;
    private EventBinding<ContainerOpenedEvent> ContainerOpenedBinding;
    private EventBinding<ContainerClosedEvent> ContainerClosedBinding;
}
