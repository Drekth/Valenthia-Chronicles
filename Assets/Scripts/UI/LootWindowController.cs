using UnityEngine;
using UnityEngine.UIElements;

// Drives the loot window. Event-driven (no toggle key): listens for the container-opened
// channel, rebuilds the slot grid from the container via the shared ItemGridView, and transfers
// clicked stacks into the player's bag. Window chrome (border, header, close) and lifecycle come
// from UIWindowController; closing raises the container-closed channel.
public class LootWindowController : UIWindowController
{
    ////////////////////////////////////////////////////////////
    /// Protected                                            ///
    ////////////////////////////////////////////////////////////

    protected override void OnBindContent(VisualElement Root)
    {
        ItemsContainer = Root.Q<VisualElement>("Items");

        Button TakeAllButton = Root.Q<Button>("TakeAll");
        if (TakeAllButton != null)
        {
            TakeAllButton.clicked += HandleTakeAll;
        }
    }

    protected override void OnSubscribe()
    {
        ContainerOpenedBinding = new EventBinding<ContainerOpenedEvent>(HandleOpen);
        EventBus<ContainerOpenedEvent>.Register(ContainerOpenedBinding);
    }

    protected override void OnUnsubscribe()
    {
        EventBus<ContainerOpenedEvent>.Deregister(ContainerOpenedBinding);
    }

    // The shared Hide() raises this once the window actually closes. Clear the active container
    // and announce the closure so the rest of the game reacts.
    protected override void OnHidden()
    {
        ActiveContainer = null;
        EventBus<ContainerClosedEvent>.Raise(new ContainerClosedEvent());
    }

    protected override void Rebuild()
    {
        if (ActiveContainer == null)
        {
            return;
        }

        ItemGridView.Rebuild(ItemsContainer, ActiveContainer.Items, ActiveContainer.SlotCount, TakeSlot);
    }

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void HandleOpen(ContainerOpenedEvent Event)
    {
        ActiveContainer = Event.Container;
        Show();
    }

    private void TakeSlot(int Index)
    {
        if (ActiveContainer == null)
        {
            return;
        }

        ItemStack Stack = ActiveContainer.TakeAt(Index);
        if (Stack.Item == null)
        {
            return;
        }

        if (ServiceLocator.TryGet<PlayerInventory>(out PlayerInventory Inventory))
        {
            Inventory.Bag.TryAdd(Stack);
        }

        Rebuild();
    }

    private void HandleTakeAll()
    {
        if (ActiveContainer == null)
        {
            return;
        }

        if (ServiceLocator.TryGet<PlayerInventory>(out PlayerInventory Inventory))
        {
            while (ActiveContainer.Items.Count > 0)
            {
                Inventory.Bag.TryAdd(ActiveContainer.TakeAt(0));
            }
        }

        Hide();
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    private VisualElement ItemsContainer;
    private Container ActiveContainer;
    private EventBinding<ContainerOpenedEvent> ContainerOpenedBinding;
}
