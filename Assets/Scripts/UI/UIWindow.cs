using System;
using UnityEngine.UIElements;

// Shared window chrome for every UI Toolkit panel (inventory, equipment, loot, future windows).
// Draws a framed panel with a centered header bar holding the title and a close button in the
// top-right corner, plus a body that receives whatever content the UXML declares between the
// element's tags. Size is customisable from UXML via the window-width / window-height
// attributes. The header doubles as the drag handle so the window can be moved by holding the
// left mouse button (see the drag region below).
[UxmlElement]
public partial class UIWindow : VisualElement
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    // Raised when the close button is clicked. The owning controller decides what to do (hide).
    public event Action CloseRequested;

    // Window title shown centered in the header bar.
    [UxmlAttribute]
    public string Title
    {
        get { return TitleValue; }
        set
        {
            TitleValue = value;
            if (TitleLabel != null)
            {
                TitleLabel.text = value;
            }
        }
    }

    // Fixed window width in pixels. Zero leaves the USS default (min-width) in charge.
    [UxmlAttribute]
    public float WindowWidth
    {
        get { return WindowWidthValue; }
        set
        {
            WindowWidthValue = value;
            ApplySize();
        }
    }

    // Fixed window height in pixels. Zero lets the content drive the height.
    [UxmlAttribute]
    public float WindowHeight
    {
        get { return WindowHeightValue; }
        set
        {
            WindowHeightValue = value;
            ApplySize();
        }
    }

    // Content authored between <UIWindow>...</UIWindow> lands in the body, not next to the header.
    public override VisualElement contentContainer
    {
        get { return Body; }
    }

    public UIWindow()
    {
        AddToClassList(WindowClass);

        Header = new VisualElement();
        Header.AddToClassList(HeaderClass);

        TitleLabel = new Label();
        TitleLabel.AddToClassList(TitleClass);
        TitleLabel.pickingMode = PickingMode.Ignore;
        Header.Add(TitleLabel);

        CloseButton = new Button();
        CloseButton.AddToClassList(CloseClass);
        CloseButton.text = "X";
        CloseButton.clicked += HandleCloseClicked;
        Header.Add(CloseButton);

        Body = new VisualElement();
        Body.AddToClassList(BodyClass);

        // Add through the hierarchy so they bypass the overridden contentContainer.
        hierarchy.Add(Header);
        hierarchy.Add(Body);

        RegisterDragCallbacks();
    }

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void HandleCloseClicked()
    {
        if (CloseRequested != null)
        {
            CloseRequested();
        }
    }

    private void ApplySize()
    {
        if (WindowWidthValue > 0.0f)
        {
            style.width = WindowWidthValue;
        }
        else
        {
            style.width = StyleKeyword.Null;
        }

        if (WindowHeightValue > 0.0f)
        {
            style.height = WindowHeightValue;
        }
        else
        {
            style.height = StyleKeyword.Null;
        }
    }

    ////////////////////////////////////////////////////////////
    /// Drag                                                 ///
    ////////////////////////////////////////////////////////////

    // Lets the window be moved by holding the left mouse button on the header. On the first move
    // the panel switches from its centered layout to absolute positioning so it stays wherever
    // it is dropped.
    private void RegisterDragCallbacks()
    {
        Header.RegisterCallback<PointerDownEvent>(HandlePointerDown);
        Header.RegisterCallback<PointerMoveEvent>(HandlePointerMove);
        Header.RegisterCallback<PointerUpEvent>(HandlePointerUp);
    }

    private void HandlePointerDown(PointerDownEvent Event)
    {
        if (Event.button != 0)
        {
            return;
        }

        IsDragging = true;
        PointerStart = Event.position;

        // Detach from the centering layout so left/top take effect, anchoring on the current
        // resolved position so the window does not jump on the first move.
        WindowStart = new UnityEngine.Vector2(layout.x, layout.y);
        style.position = Position.Absolute;
        style.left = WindowStart.x;
        style.top = WindowStart.y;

        Header.CapturePointer(Event.pointerId);
        Event.StopPropagation();
    }

    private void HandlePointerMove(PointerMoveEvent Event)
    {
        if (!IsDragging)
        {
            return;
        }

        UnityEngine.Vector2 Delta = (UnityEngine.Vector2)Event.position - PointerStart;
        style.left = WindowStart.x + Delta.x;
        style.top = WindowStart.y + Delta.y;
        Event.StopPropagation();
    }

    private void HandlePointerUp(PointerUpEvent Event)
    {
        if (!IsDragging)
        {
            return;
        }

        IsDragging = false;
        Header.ReleasePointer(Event.pointerId);
        Event.StopPropagation();
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    private const string WindowClass = "ui-window";
    private const string HeaderClass = "ui-window__header";
    private const string TitleClass  = "ui-window__title";
    private const string CloseClass  = "ui-window__close";
    private const string BodyClass   = "ui-window__body";

    private readonly VisualElement Header;
    private readonly Label TitleLabel;
    private readonly Button CloseButton;
    private readonly VisualElement Body;

    private string TitleValue;
    private float WindowWidthValue;
    private float WindowHeightValue;

    private bool IsDragging;
    private UnityEngine.Vector2 PointerStart;
    private UnityEngine.Vector2 WindowStart;
}
