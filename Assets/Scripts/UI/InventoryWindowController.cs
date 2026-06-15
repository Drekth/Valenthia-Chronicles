using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

// Player inventory window (UI Toolkit, InventoryWindow.uxml / ItemWindow.uss). A key toggles
// it; when shown it draws the player's bag — resolved from the ServiceLocator — as a fixed
// slot grid using the shared ItemGridView. Owns a self-contained toggle InputAction so it
// stays independent of the shared input asset. Read-only for now (no take/drop).
[RequireComponent(typeof(UIDocument))]
public class InventoryWindowController : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void Awake()
    {
        Document = GetComponent<UIDocument>();
        ToggleAction = new InputAction("ToggleInventory", InputActionType.Button, ToggleBinding);
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
    }

    private void OnDisable()
    {
        if (ToggleAction != null)
        {
            ToggleAction.Disable();
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

        Window = Root.Q<VisualElement>("InventoryWindow");
        ItemsContainer = Root.Q<VisualElement>("Items");

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
        if (!ServiceLocator.TryGet<PlayerInventory>(out PlayerInventory Inventory))
        {
            return;
        }

        Container Bag = Inventory.Bag;
        ItemGridView.Rebuild(ItemsContainer, Bag.Items, Bag.SlotCount, null);
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
    [SerializeField] private string ToggleBinding = "<Keyboard>/i";

    private UIDocument Document;
    private InputAction ToggleAction;
    private VisualElement Window;
    private VisualElement ItemsContainer;
    private bool IsShown;
}
