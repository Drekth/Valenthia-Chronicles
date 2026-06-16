using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

// Base class for every UI Toolkit window controller (inventory, equipment, loot, future ones).
// Factors out the shared lifecycle: resolving the UIDocument, finding the shared UIWindow chrome,
// wiring its close button, an optional self-contained toggle InputAction, and Show/Hide/IsShown.
// Subclasses only provide their content binding and rebuild logic through the hooks below.
[RequireComponent(typeof(UIDocument))]
public abstract class UIWindowController : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public bool IsShown { get; private set; }

    ////////////////////////////////////////////////////////////
    /// Protected                                            ///
    ////////////////////////////////////////////////////////////

    // Rebuilds the window content from the current model. Called every time the window is shown.
    protected abstract void Rebuild();

    // Grabs the content elements the subclass needs (called once the visual tree is bound).
    protected virtual void OnBindContent(VisualElement Root)
    {
    }

    // Subscribe / unsubscribe to SO event channels in these hooks (paired with enable/disable).
    protected virtual void OnSubscribe()
    {
    }

    protected virtual void OnUnsubscribe()
    {
    }

    // Called after the window transitions from shown to hidden (never on the initial hide).
    protected virtual void OnHidden()
    {
    }

    protected void Show()
    {
        Rebuild();

        if (Window != null)
        {
            Window.style.display = DisplayStyle.Flex;
        }

        IsShown = true;
    }

    protected void Hide()
    {
        bool WasShown = IsShown;

        if (Window != null)
        {
            Window.style.display = DisplayStyle.None;
        }

        IsShown = false;

        if (WasShown)
        {
            OnHidden();
        }
    }

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    protected virtual void Awake()
    {
        Document = GetComponent<UIDocument>();

        if (!string.IsNullOrEmpty(ToggleBinding))
        {
            ToggleAction = new InputAction("ToggleWindow", InputActionType.Button, ToggleBinding);
            ToggleAction.performed += HandleToggle;
        }
    }

    protected virtual void OnEnable()
    {
        BindVisualTree();
        Hide();

        if (ToggleAction != null)
        {
            ToggleAction.Enable();
        }

        OnSubscribe();
    }

    protected virtual void OnDisable()
    {
        if (ToggleAction != null)
        {
            ToggleAction.Disable();
        }

        OnUnsubscribe();
    }

    protected virtual void OnDestroy()
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

        Window = Root.Q<UIWindow>();
        if (Window != null)
        {
            Window.CloseRequested += Hide;
        }

        OnBindContent(Root);
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

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    // Optional toggle key (e.g. "<Keyboard>/i"). Leave empty for event-driven windows (loot).
    [Header("Input")]
    [SerializeField] private string ToggleBinding = "";

    private UIDocument Document;
    private InputAction ToggleAction;
    private UIWindow Window;
}
