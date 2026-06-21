using UnityEngine;
using UnityEngine.UIElements;

// Drives the player HUD action bar built with UI Toolkit (HUDActionBar.uxml / .uss).
// Queries the visual tree on enable, then updates resource bars and spell-slot icons.
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
        if (HealthFill == null) { return; }

        HealthFill.style.width = Length.Percent(Mathf.Clamp01(Percent) * 100f);
    }

    public void SetManaPercent(float Percent)
    {
        if (ManaFill == null) { return; }

        ManaFill.style.width = Length.Percent(Mathf.Clamp01(Percent) * 100f);
    }

    public void SetSlotIcon(int Index, Sprite Icon)
    {
        if (Index < 0 || Index >= SlotIcons.Length) { return; }

        VisualElement Target = SlotIcons[Index];
        if (Target == null) { return; }

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
        RefreshPlayerHealth();

        SlotAssignedBinding = new EventBinding<HotbarSlotAssignedEvent>(HandleSlotAssigned);
        EventBus<HotbarSlotAssignedEvent>.Register(SlotAssignedBinding);

        DamageTakenBinding = new EventBinding<DamageTakenEvent>(HandleDamageTaken);
        EventBus<DamageTakenEvent>.Register(DamageTakenBinding);
    }

    private void OnDisable()
    {
        EventBus<HotbarSlotAssignedEvent>.Deregister(SlotAssignedBinding);
        EventBus<DamageTakenEvent>.Deregister(DamageTakenBinding);
    }

    private void HandleSlotAssigned(HotbarSlotAssignedEvent Event)
    {
        SetSlotIcon(Event.Slot, Event.Spell != null ? Event.Spell.Icon : null);
    }

    private void HandleDamageTaken(DamageTakenEvent Event)
    {
        Unit Player = GetPlayerUnit();
        if (Event.Target != Player)
        {
            return;
        }

        SetHealthPercent(Player.CurrentHealth / Player.MaximumHealth);
    }

    private void RefreshPlayerHealth()
    {
        Unit Player = GetPlayerUnit();
        if (Player != null)
        {
            SetHealthPercent(Player.CurrentHealth / Player.MaximumHealth);
        }
        else
        {
            SetHealthPercent(1.0f);
        }

        SetManaPercent(1.0f);
    }

    private Unit GetPlayerUnit()
    {
        if (PlayerUnit == null)
        {
            GameObject PlayerGO = GameObject.FindGameObjectWithTag("Player");
            if (PlayerGO != null)
            {
                PlayerUnit = PlayerGO.GetComponent<Unit>();
            }
        }

        return PlayerUnit;
    }

    private void BindVisualTree()
    {
        VisualElement Root = Document.rootVisualElement;
        if (Root == null) { return; }

        HealthFill = Root.Q<VisualElement>("HealthFill");
        ManaFill   = Root.Q<VisualElement>("ManaFill");

        for (int I = 0; I < SlotCount; I++)
        {
            VisualElement Slot = Root.Q<VisualElement>("Slot" + (I + 1));
            SlotIcons[I] = Slot != null ? Slot.Q<VisualElement>("Icon") : null;
        }
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    private UIDocument Document;
    private Unit PlayerUnit;
    private EventBinding<HotbarSlotAssignedEvent> SlotAssignedBinding;
    private EventBinding<DamageTakenEvent>        DamageTakenBinding;
    private VisualElement HealthFill;
    private VisualElement ManaFill;
    private VisualElement[] SlotIcons = new VisualElement[SlotCount];
}
