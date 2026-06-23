using UnityEngine;
using UnityEngine.UIElements;

// Drives the player HUD action bar built with UI Toolkit (HUDActionBar.uxml / .uss).
// Queries the visual tree on enable, then keeps resource bars and spell-slot icons up to date
// by reacting to events — no polling.
[RequireComponent(typeof(UIDocument))]
public class HUDActionBar : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Constants                                            ///
    ////////////////////////////////////////////////////////////

    public const int SlotCount = 10;

    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public void SetHealthPercent(float Percent)
    {
        if (HealthFill == null)
        {
            return;
        }

        HealthFill.style.width = Length.Percent(Mathf.Clamp01(Percent) * 100f);
    }

    public void SetManaPercent(float Percent)
    {
        if (ManaFill == null)
        {
            return;
        }

        ManaFill.style.width = Length.Percent(Mathf.Clamp01(Percent) * 100f);
    }

    public void SetSlotIcon(int Index, Sprite Icon)
    {
        if (Index < 0 || Index >= SlotIcons.Length)
        {
            return;
        }

        VisualElement Target = SlotIcons[Index];
        if (Target == null)
        {
            return;
        }

        Target.style.backgroundImage = Icon != null ? new StyleBackground(Icon) : new StyleBackground();
    }

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void Awake()
    {
        Document = GetComponent<UIDocument>();
    }

    private void OnEnable()
    {
        BindVisualTree();

        PlayerSpawnedBinding = new EventBinding<PlayerSpawnedEvent>(HandlePlayerSpawned);
        EventBus<PlayerSpawnedEvent>.Register(PlayerSpawnedBinding);

        SlotAssignedBinding = new EventBinding<HotbarSlotAssignedEvent>(HandleSlotAssigned);
        EventBus<HotbarSlotAssignedEvent>.Register(SlotAssignedBinding);

        HealthChangedBinding = new EventBinding<UnitHealthChangedEvent>(HandleHealthChanged);
        EventBus<UnitHealthChangedEvent>.Register(HealthChangedBinding);

        ManaChangedBinding = new EventBinding<ManaChangedEvent>(HandleManaChanged);
        EventBus<ManaChangedEvent>.Register(ManaChangedBinding);

        SpellDragDroppedBinding = new EventBinding<SpellDragDroppedEvent>(HandleSpellDragDropped);
        EventBus<SpellDragDroppedEvent>.Register(SpellDragDroppedBinding);

        // Snapshot current state in case the player was already spawned before this enable.
        RefreshFromPlayerUnit();
    }

    private void OnDisable()
    {
        EventBus<PlayerSpawnedEvent>.Deregister(PlayerSpawnedBinding);
        EventBus<HotbarSlotAssignedEvent>.Deregister(SlotAssignedBinding);
        EventBus<UnitHealthChangedEvent>.Deregister(HealthChangedBinding);
        EventBus<ManaChangedEvent>.Deregister(ManaChangedBinding);
        EventBus<SpellDragDroppedEvent>.Deregister(SpellDragDroppedBinding);
    }

    private void HandlePlayerSpawned(PlayerSpawnedEvent Event)
    {
        PlayerUnit = Event.Character != null ? Event.Character.GetComponent<Unit>() : null;
        RefreshFromPlayerUnit();
    }

    private void HandleSlotAssigned(HotbarSlotAssignedEvent Event)
    {
        SetSlotIcon(Event.Slot, Event.Spell != null ? Event.Spell.Icon : null);
    }

    private void HandleHealthChanged(UnitHealthChangedEvent Event)
    {
        if (Event.Target != PlayerUnit)
        {
            return;
        }

        SetHealthPercent(Event.CurrentHealth / Event.MaxHealth);
    }

    private void HandleManaChanged(ManaChangedEvent Event)
    {
        if (Event.Target != PlayerUnit)
        {
            return;
        }

        SetManaPercent(Event.CurrentMana / Event.MaxMana);
    }

    // A spell was dropped from the spellbook: find which slot (if any) the pointer landed on and
    // request its assignment. Positions are in shared panel space, so they compare directly to the
    // slot worldBounds. The actual binding and icon repaint flow back through PlayerHotbar.
    private void HandleSpellDragDropped(SpellDragDroppedEvent Event)
    {
        for (int Index = 0; Index < SlotCount; Index++)
        {
            VisualElement Slot = Slots[Index];
            if (Slot == null || !Slot.worldBound.Contains(Event.PanelPosition))
            {
                continue;
            }

            EventBus<HotbarAssignRequestedEvent>.Raise(new HotbarAssignRequestedEvent
            {
                Slot  = Index,
                Spell = Event.Spell,
            });
            return;
        }
    }

    private void RefreshFromPlayerUnit()
    {
        if (PlayerUnit != null)
        {
            SetHealthPercent(PlayerUnit.CurrentHealth / PlayerUnit.MaximumHealth);
            SetManaPercent(PlayerUnit.CurrentMana / PlayerUnit.MaximumMana);
        }
        else
        {
            SetHealthPercent(1.0f);
            SetManaPercent(1.0f);
        }
    }

    private void BindVisualTree()
    {
        VisualElement Root = Document.rootVisualElement;
        if (Root == null)
        {
            return;
        }

        HealthFill = Root.Q<VisualElement>("HealthFill");
        ManaFill   = Root.Q<VisualElement>("ManaFill");

        for (int I = 0; I < SlotCount; I++)
        {
            VisualElement Slot = Root.Q<VisualElement>("Slot" + (I + 1));
            Slots[I]     = Slot;
            SlotIcons[I] = Slot != null ? Slot.Q<VisualElement>("Icon") : null;
        }
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    private UIDocument Document;
    private Unit PlayerUnit;

    private EventBinding<PlayerSpawnedEvent>      PlayerSpawnedBinding;
    private EventBinding<HotbarSlotAssignedEvent> SlotAssignedBinding;
    private EventBinding<UnitHealthChangedEvent>  HealthChangedBinding;
    private EventBinding<ManaChangedEvent>        ManaChangedBinding;
    private EventBinding<SpellDragDroppedEvent>   SpellDragDroppedBinding;

    private VisualElement   HealthFill;
    private VisualElement   ManaFill;
    private VisualElement[] Slots     = new VisualElement[SlotCount];
    private VisualElement[] SlotIcons = new VisualElement[SlotCount];
}
