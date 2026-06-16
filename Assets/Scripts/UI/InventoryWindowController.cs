using UnityEngine;
using UnityEngine.UIElements;

// Player inventory window. Toggled by a key (see ToggleBinding); when shown it draws the
// player's bag — resolved from the ServiceLocator — as a fixed slot grid using the shared
// ItemGridView. Window chrome (border, header, close) and lifecycle come from UIWindowController.
// Clicking a bag item equips it, swapping any worn piece back into the bag.
public class InventoryWindowController : UIWindowController
{
    ////////////////////////////////////////////////////////////
    /// Protected                                            ///
    ////////////////////////////////////////////////////////////

    protected override void OnBindContent(VisualElement Root)
    {
        ItemsContainer = Root.Q<VisualElement>("Items");
    }

    protected override void OnSubscribe()
    {
        if (OnEquipmentChanged != null)
        {
            OnEquipmentChanged.Subscribe(HandleEquipmentChanged);
        }
    }

    protected override void OnUnsubscribe()
    {
        if (OnEquipmentChanged != null)
        {
            OnEquipmentChanged.Unsubscribe(HandleEquipmentChanged);
        }
    }

    protected override void Rebuild()
    {
        if (!ServiceLocator.TryGet<PlayerInventory>(out PlayerInventory Inventory))
        {
            return;
        }

        Container Bag = Inventory.Bag;
        ItemGridView.Rebuild(ItemsContainer, Bag.Items, Bag.SlotCount, HandleSlotClicked);
    }

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void HandleEquipmentChanged()
    {
        if (IsShown)
        {
            Rebuild();
        }
    }

    // Equips the clicked bag item into its slot, swapping any worn piece back into the bag.
    // Non-equippable items are ignored for now. Announces the change so the equipment window
    // refreshes too.
    private void HandleSlotClicked(int Index)
    {
        if (!ServiceLocator.TryGet<PlayerInventory>(out PlayerInventory Inventory))
        {
            return;
        }

        Container Bag = Inventory.Bag;
        if (Index < 0 || Index >= Bag.Items.Count)
        {
            return;
        }

        ItemData Item = Bag.Items[Index].Item;
        if (Item == null || !Item.IsEquippable)
        {
            return;
        }

        if (!ServiceLocator.TryGet<Equipment>(out Equipment Gear))
        {
            return;
        }

        ItemStack Stack = Bag.TakeAt(Index);

        if (!Gear.TryEquip(Stack.Item, out ItemData Displaced))
        {
            Bag.TryAdd(Stack);
            return;
        }

        // Equippable stacks are single, but return any remainder just in case.
        if (Stack.Amount > 1)
        {
            Bag.TryAdd(new ItemStack(Stack.Item, Stack.Amount - 1));
        }

        if (Displaced != null)
        {
            Bag.TryAdd(new ItemStack(Displaced, 1));
        }

        if (OnEquipmentChanged != null)
        {
            OnEquipmentChanged.Raise();
        }

        Rebuild();
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [Header("Events")]
    [SerializeField] private VoidEventChannel OnEquipmentChanged;

    private VisualElement ItemsContainer;
}
