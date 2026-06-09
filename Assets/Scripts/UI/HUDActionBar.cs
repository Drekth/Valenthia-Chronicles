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
        ApplyDemoState();
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

    private void ApplyDemoState()
    {
        // Placeholder resource levels until the health/mana systems drive the bars.
        // Spell slots start empty; icons are assigned at runtime via SetSlotIcon.
        SetHealthPercent(0.75f);
        SetManaPercent(0.55f);
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    private UIDocument Document;
    private VisualElement HealthFill;
    private VisualElement ManaFill;
    private VisualElement[] SlotIcons = new VisualElement[SlotCount];
}
