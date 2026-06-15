using UnityEngine;
using UnityEngine.UIElements;

// Drives the loot window built with UI Toolkit (LootWindow.uxml / ItemWindow.uss). Listens
// for the container-opened channel, rebuilds the slot grid from the container via the shared
// ItemGridView, and transfers clicked stacks into the player's bag. Hidden until opened.
[RequireComponent(typeof(UIDocument))]
public class LootWindowController : MonoBehaviour
{
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
        Hide();

        if (OnContainerOpened != null)
        {
            OnContainerOpened.Subscribe(HandleOpen);
        }
    }

    private void OnDisable()
    {
        if (OnContainerOpened != null)
        {
            OnContainerOpened.Unsubscribe(HandleOpen);
        }
    }

    private void BindVisualTree()
    {
        VisualElement Root = Document.rootVisualElement;
        if (Root == null)
        {
            return;
        }

        Window = Root.Q<VisualElement>("LootWindow");
        ItemsContainer = Root.Q<VisualElement>("Items");

        Button TakeAllButton = Root.Q<Button>("TakeAll");
        if (TakeAllButton != null)
        {
            TakeAllButton.clicked += HandleTakeAll;
        }

        Button CloseButton = Root.Q<Button>("Close");
        if (CloseButton != null)
        {
            CloseButton.clicked += Close;
        }
    }

    private void HandleOpen(Container Opened)
    {
        ActiveContainer = Opened;
        Rebuild();
        Show();
    }

    private void Rebuild()
    {
        if (ActiveContainer == null)
        {
            return;
        }

        ItemGridView.Rebuild(ItemsContainer, ActiveContainer.Items, ActiveContainer.SlotCount, TakeSlot);
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

        Close();
    }

    private void Show()
    {
        if (Window != null)
        {
            Window.style.display = DisplayStyle.Flex;
        }
    }

    private void Hide()
    {
        if (Window != null)
        {
            Window.style.display = DisplayStyle.None;
        }
    }

    private void Close()
    {
        ActiveContainer = null;
        Hide();

        if (OnContainerClosed != null)
        {
            OnContainerClosed.Raise();
        }
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [Header("Events")]
    [SerializeField] private ContainerEventChannel OnContainerOpened;
    [SerializeField] private VoidEventChannel OnContainerClosed;

    private UIDocument Document;
    private VisualElement Window;
    private VisualElement ItemsContainer;
    private Container ActiveContainer;
}
