using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

// Player equipment window (UI Toolkit, EquipmentWindow.uxml / ItemWindow.uss). A key toggles
// it; when shown it fills a fixed paper-doll of named slots from the Equipment model resolved
// through the ServiceLocator. Clicking a worn slot unequips it back into the bag (blocked when
// the bag is full). Stays in sync with equips made elsewhere by listening to the changed
// channel. Owns a self-contained toggle InputAction so it stays independent of the shared
// input asset.
[RequireComponent(typeof(UIDocument))]
public class EquipmentWindowController : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void Awake()
    {
        Document = GetComponent<UIDocument>();
        ToggleAction = new InputAction("ToggleEquipment", InputActionType.Button, ToggleBinding);
        ToggleAction.performed += HandleToggle;
    }

    private void OnEnable()
    {
        BindVisualTree();
        Hide();

        if (ToggleAction != null)
        {
            ToggleAction.Enable();
        }

        if (OnEquipmentChanged != null)
        {
            OnEquipmentChanged.Subscribe(HandleEquipmentChanged);
        }
    }

    private void OnDisable()
    {
        if (ToggleAction != null)
        {
            ToggleAction.Disable();
        }

        if (OnEquipmentChanged != null)
        {
            OnEquipmentChanged.Unsubscribe(HandleEquipmentChanged);
        }
    }

    private void OnDestroy()
    {
        if (ToggleAction != null)
        {
            ToggleAction.performed -= HandleToggle;
            ToggleAction.Dispose();
        }
    }

    private void BindVisualTree()
    {
        VisualElement Root = Document.rootVisualElement;
        if (Root == null)
        {
            return;
        }

        Window = Root.Q<VisualElement>("EquipmentWindow");

        SlotElements = new VisualElement[SlotMap.Length];
        for (int I = 0; I < SlotMap.Length; I++)
        {
            VisualElement Element = Root.Q<VisualElement>(SlotMap[I].ElementName);
            SlotElements[I] = Element;

            if (Element == null)
            {
                continue;
            }

            // Capture into locals so each slot's click closure keeps its own target.
            EquipmentSlot Slot = SlotMap[I].Slot;
            int RingIndex = SlotMap[I].RingIndex;
            Element.RegisterCallback<ClickEvent>(Event => UnequipSlot(Slot, RingIndex));
        }

        Button CloseButton = Root.Q<Button>("Close");
        if (CloseButton != null)
        {
            CloseButton.clicked += Hide;
        }
    }

    private void HandleToggle(InputAction.CallbackContext Context)
    {
        if (IsShown)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }

    private void HandleEquipmentChanged()
    {
        if (IsShown)
        {
            Rebuild();
        }
    }

    private void Show()
    {
        Rebuild();

        if (Window != null)
        {
            Window.style.display = DisplayStyle.Flex;
        }

        IsShown = true;
    }

    private void Rebuild()
    {
        if (SlotElements == null)
        {
            return;
        }

        if (!ServiceLocator.TryGet<Equipment>(out Equipment Gear))
        {
            return;
        }

        for (int I = 0; I < SlotMap.Length; I++)
        {
            if (SlotElements[I] == null)
            {
                continue;
            }

            ItemData Worn = Gear.Get(SlotMap[I].Slot, SlotMap[I].RingIndex);
            FillSlot(SlotElements[I], Worn);
        }
    }

    // Paints a single doll slot from its worn item: icon as background, name as fallback, or
    // an empty cell when nothing is worn.
    private void FillSlot(VisualElement Slot, ItemData Item)
    {
        Slot.Clear();
        Slot.style.backgroundImage = StyleKeyword.Null;

        if (Item == null)
        {
            return;
        }

        if (Item.Icon != null)
        {
            Slot.style.backgroundImage = new StyleBackground(Item.Icon);
        }
        else
        {
            Label Name = new Label(Item.DisplayName);
            Name.AddToClassList("item-slot__name");
            Name.pickingMode = PickingMode.Ignore;
            Slot.Add(Name);
        }
    }

    // Moves the worn item back to the bag and clears the slot. Blocked when the bag is full so
    // the item is never destroyed. Announces the change so the inventory window refreshes too.
    private void UnequipSlot(EquipmentSlot Slot, int RingIndex)
    {
        if (!ServiceLocator.TryGet<Equipment>(out Equipment Gear))
        {
            return;
        }

        ItemData Worn = Gear.Get(Slot, RingIndex);
        if (Worn == null)
        {
            return;
        }

        if (!ServiceLocator.TryGet<PlayerInventory>(out PlayerInventory Inventory))
        {
            return;
        }

        if (!Inventory.Bag.TryAdd(new ItemStack(Worn, 1)))
        {
            return;
        }

        Gear.TryUnequip(Slot, RingIndex, out ItemData Removed);

        if (OnEquipmentChanged != null)
        {
            OnEquipmentChanged.Raise();
        }

        Rebuild();
    }

    private void Hide()
    {
        if (Window != null)
        {
            Window.style.display = DisplayStyle.None;
        }

        IsShown = false;
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [Header("Input")]
    [SerializeField] private string ToggleBinding = "<Keyboard>/c";

    [Header("Events")]
    [SerializeField] private VoidEventChannel OnEquipmentChanged;

    private UIDocument Document;
    private InputAction ToggleAction;
    private VisualElement Window;
    private VisualElement[] SlotElements;
    private bool IsShown;

    // Maps each named doll slot in the UXML to its model address. Ring1/Ring2 share the Ring
    // slot and differ only by ring index.
    private readonly SlotBinding[] SlotMap =
    {
        new SlotBinding("Slot_Head",     EquipmentSlot.Head,     0),
        new SlotBinding("Slot_Shoulder", EquipmentSlot.Shoulder, 0),
        new SlotBinding("Slot_Chest",    EquipmentSlot.Chest,    0),
        new SlotBinding("Slot_Hands",    EquipmentSlot.Hands,    0),
        new SlotBinding("Slot_Legs",     EquipmentSlot.Legs,     0),
        new SlotBinding("Slot_Neck",     EquipmentSlot.Neck,     0),
        new SlotBinding("Slot_Ring1",    EquipmentSlot.Ring,     0),
        new SlotBinding("Slot_Ring2",    EquipmentSlot.Ring,     1),
        new SlotBinding("Slot_OffHand",  EquipmentSlot.OffHand,  0),
        new SlotBinding("Slot_MainHand", EquipmentSlot.MainHand, 0),
    };

    ////////////////////////////////////////////////////////////
    /// Types                                                ///
    ////////////////////////////////////////////////////////////

    // Binds a UXML slot element name to the Equipment address it displays.
    private readonly struct SlotBinding
    {
        public SlotBinding(string Name, EquipmentSlot TargetSlot, int Ring)
        {
            ElementName = Name;
            Slot = TargetSlot;
            RingIndex = Ring;
        }

        public string        ElementName { get; }
        public EquipmentSlot Slot        { get; }
        public int           RingIndex   { get; }
    }
}
