using UnityEngine;

// Authority for the player's action-bar loadout: the spell bound to each of the ten slots. Lives on
// the persistent player rig (not the per-zone body) so the loadout survives zone swaps, and is
// published through the ServiceLocator so PlayerCharacter.ResolveSlot can read it when casting.
//
// The loadout starts from DefaultLoadout (slot 0 is the basic attack) and is mutated at runtime by
// the spellbook drag-and-drop: a drop raises HotbarAssignRequestedEvent, which we apply and then
// mirror to the HUD with HotbarSlotAssignedEvent. The loadout is re-published whenever a body spawns
// so the HUD repaints its icons once it (and the body) are ready.
public class PlayerHotbar : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    // The spell bound to a slot, or null when the slot is empty or the index is out of range.
    public SpellData GetSlot(int Slot)
    {
        if (Slot < 0 || Slot >= Slots.Length)
        {
            return null;
        }

        return Slots[Slot];
    }

    // Binds a spell to a slot (null clears it) and mirrors the change to the HUD.
    public void Assign(int Slot, SpellData Spell)
    {
        if (Slot < 0 || Slot >= Slots.Length)
        {
            return;
        }

        Slots[Slot] = Spell;

        EventBus<HotbarSlotAssignedEvent>.Raise(new HotbarSlotAssignedEvent
        {
            Slot  = Slot,
            Spell = Spell,
        });
    }

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void Awake()
    {
        InitialiseSlots();
        ServiceLocator.Register<PlayerHotbar>(this);
    }

    private void OnEnable()
    {
        AssignRequestedBinding = new EventBinding<HotbarAssignRequestedEvent>(HandleAssignRequested);
        EventBus<HotbarAssignRequestedEvent>.Register(AssignRequestedBinding);

        // The HUD lives in the persistent UI scene and the body spawns after it, so re-publishing the
        // loadout on spawn guarantees the slot icons paint even though this rig initialised earlier.
        PlayerSpawnedBinding = new EventBinding<PlayerSpawnedEvent>(HandlePlayerSpawned);
        EventBus<PlayerSpawnedEvent>.Register(PlayerSpawnedBinding);
    }

    private void OnDisable()
    {
        EventBus<HotbarAssignRequestedEvent>.Deregister(AssignRequestedBinding);
        EventBus<PlayerSpawnedEvent>.Deregister(PlayerSpawnedBinding);
    }

    private void OnDestroy()
    {
        if (ServiceLocator.TryGet<PlayerHotbar>(out PlayerHotbar Current) && Current == this)
        {
            ServiceLocator.Unregister<PlayerHotbar>();
        }
    }

    // Copies the authored DefaultLoadout into the runtime slots, padding the rest with empty slots.
    private void InitialiseSlots()
    {
        for (int Slot = 0; Slot < Slots.Length; Slot++)
        {
            Slots[Slot] = Slot < DefaultLoadout.Length ? DefaultLoadout[Slot] : null;
        }
    }

    private void HandleAssignRequested(HotbarAssignRequestedEvent Event)
    {
        Assign(Event.Slot, Event.Spell);
    }

    private void HandlePlayerSpawned(PlayerSpawnedEvent Event)
    {
        // Only repaint when a body is actually present; the despawn (null) keeps the loadout intact.
        if (Event.Character != null)
        {
            PublishAll();
        }
    }

    // Raises one HotbarSlotAssignedEvent per slot so the HUD mirrors the whole loadout exactly.
    private void PublishAll()
    {
        for (int Slot = 0; Slot < Slots.Length; Slot++)
        {
            EventBus<HotbarSlotAssignedEvent>.Raise(new HotbarSlotAssignedEvent
            {
                Slot  = Slot,
                Spell = Slots[Slot],
            });
        }
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [SerializeField] private SpellData[] DefaultLoadout = new SpellData[0];

    private readonly SpellData[] Slots = new SpellData[HUDActionBar.SlotCount];

    private EventBinding<HotbarAssignRequestedEvent> AssignRequestedBinding;
    private EventBinding<PlayerSpawnedEvent>         PlayerSpawnedBinding;
}
