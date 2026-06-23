using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// Floating mini health bars over creatures currently in combat. A single screen-space overlay
// (UIDocument) owns a pool of bar elements; each LateUpdate the tracked units' world positions
// are projected to panel space so the bars ride above their heads. Bars appear on
// UnitCombatStateChangedEvent (InCombat) and vanish on leave or death, so out-of-combat creatures
// stay clean. Styled to match the player resource bars (EnemyHealthBars.uss).
[RequireComponent(typeof(UIDocument))]
public class EnemyHealthBars : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Constants                                            ///
    ////////////////////////////////////////////////////////////

    // World units kept between the creature's head and the bar's bottom edge.
    private const float HeadPadding = 0.35f;

    // Fallback head height when a unit exposes no collider to measure.
    private const float FallbackHeight = 2.0f;

    // Normalized slack past the viewport edges before a bar is hidden, so bars near the screen
    // border don't pop in and out.
    private const float ViewportMargin = 0.1f;

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void Awake()
    {
        Document    = GetComponent<UIDocument>();
        ActiveBars  = new Dictionary<Unit, BarEntry>();
        Pool        = new Stack<BarEntry>();
        StaleUnits  = new List<Unit>();
    }

    private void OnEnable()
    {
        Root = Document.rootVisualElement != null
            ? Document.rootVisualElement.Q<VisualElement>("EnemyHealthBarsRoot")
            : null;

        CombatStateBinding = new EventBinding<UnitCombatStateChangedEvent>(HandleCombatStateChanged);
        EventBus<UnitCombatStateChangedEvent>.Register(CombatStateBinding);

        HealthChangedBinding = new EventBinding<UnitHealthChangedEvent>(HandleHealthChanged);
        EventBus<UnitHealthChangedEvent>.Register(HealthChangedBinding);

        UnitDiedBinding = new EventBinding<UnitDiedEvent>(HandleUnitDied);
        EventBus<UnitDiedEvent>.Register(UnitDiedBinding);
    }

    private void OnDisable()
    {
        EventBus<UnitCombatStateChangedEvent>.Deregister(CombatStateBinding);
        EventBus<UnitHealthChangedEvent>.Deregister(HealthChangedBinding);
        EventBus<UnitDiedEvent>.Deregister(UnitDiedBinding);
    }

    // Follow every tracked head once per frame, after movement has been applied for the frame.
    private void LateUpdate()
    {
        if (Root == null || ActiveBars.Count == 0)
        {
            return;
        }

        Camera View = ResolveCamera();
        if (View == null)
        {
            return;
        }

        IPanel Panel = Root.panel;

        foreach (KeyValuePair<Unit, BarEntry> Pair in ActiveBars)
        {
            Unit Owner = Pair.Key;
            if (Owner == null)
            {
                StaleUnits.Add(Owner);
                continue;
            }

            BarEntry Entry = Pair.Value;
            Vector3 WorldAnchor = Entry.GetAnchorPosition();
            Vector3 Viewport = View.WorldToViewportPoint(WorldAnchor);

            // Hide when behind the camera or off-screen, instead of letting the near-plane
            // projection fling the bar to extreme coordinates.
            if (Viewport.z <= 0.0f
                || Viewport.x < -ViewportMargin || Viewport.x > 1.0f + ViewportMargin
                || Viewport.y < -ViewportMargin || Viewport.y > 1.0f + ViewportMargin)
            {
                Entry.Bar.style.display = DisplayStyle.None;
                continue;
            }

            // Camera-aware world-to-panel projection: handles the panel's scale mode and avoids
            // the manual screen-space conversion that jittered under ScaleWithScreenSize.
            Vector2 PanelPoint = RuntimePanelUtils.CameraTransformWorldToPanel(Panel, WorldAnchor, View);
            Entry.Bar.style.left    = PanelPoint.x;
            Entry.Bar.style.top     = PanelPoint.y;
            Entry.Bar.style.display = DisplayStyle.Flex;
        }

        FlushStaleUnits();
    }

    private void HandleCombatStateChanged(UnitCombatStateChangedEvent Event)
    {
        if (Event.InCombat && Event.Unit.IsAttackable)
        {
            ShowBar(Event.Unit);
        }
        else
        {
            RemoveBar(Event.Unit);
        }
    }

    private void HandleHealthChanged(UnitHealthChangedEvent Event)
    {
        if (Event.Target == null)
        {
            return;
        }

        if (ActiveBars.TryGetValue(Event.Target, out BarEntry Entry))
        {
            SetFill(Entry, Event.CurrentHealth / Event.MaxHealth);
        }
    }

    private void HandleUnitDied(UnitDiedEvent Event)
    {
        RemoveBar(Event.Unit);
    }

    private void ShowBar(Unit Owner)
    {
        if (Owner == null || Root == null || ActiveBars.ContainsKey(Owner))
        {
            return;
        }

        BarEntry Entry = AcquireEntry();
        Entry.Bind(Owner);
        Entry.Bar.style.display = DisplayStyle.None; // Positioned on the next LateUpdate.
        SetFill(Entry, Owner.CurrentHealth / Owner.MaximumHealth);

        Root.Add(Entry.Bar);
        ActiveBars.Add(Owner, Entry);
    }

    private void RemoveBar(Unit Owner)
    {
        if (Owner == null || !ActiveBars.TryGetValue(Owner, out BarEntry Entry))
        {
            return;
        }

        ActiveBars.Remove(Owner);
        ReleaseEntry(Entry);
    }

    private void FlushStaleUnits()
    {
        for (int I = 0; I < StaleUnits.Count; I++)
        {
            RemoveBar(StaleUnits[I]);
        }

        StaleUnits.Clear();
    }

    private static void SetFill(BarEntry Entry, float Percent)
    {
        Entry.Fill.style.width = Length.Percent(Mathf.Clamp01(Percent) * 100.0f);
    }

    private BarEntry AcquireEntry()
    {
        if (Pool.Count > 0)
        {
            return Pool.Pop();
        }

        return BarEntry.Create();
    }

    private void ReleaseEntry(BarEntry Entry)
    {
        Entry.Bar.RemoveFromHierarchy();
        Entry.Unbind();
        Pool.Push(Entry);
    }

    private Camera ResolveCamera()
    {
        if (WorldCamera == null)
        {
            WorldCamera = Camera.main;
        }

        return WorldCamera;
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    private UIDocument Document;
    private VisualElement Root;
    private Camera WorldCamera;

    private Dictionary<Unit, BarEntry> ActiveBars;
    private Stack<BarEntry> Pool;
    private List<Unit> StaleUnits;

    private EventBinding<UnitCombatStateChangedEvent> CombatStateBinding;
    private EventBinding<UnitHealthChangedEvent>      HealthChangedBinding;
    private EventBinding<UnitDiedEvent>               UnitDiedBinding;

    ////////////////////////////////////////////////////////////
    /// Bar entry                                            ///
    ////////////////////////////////////////////////////////////

    // One pooled bar plus the cached transform/collider it tracks, so per-frame positioning stays
    // allocation-free.
    private class BarEntry
    {
        public VisualElement Bar;
        public VisualElement Fill;

        public static BarEntry Create()
        {
            VisualElement Bar = new VisualElement();
            Bar.AddToClassList("enemy-bar");
            Bar.pickingMode = PickingMode.Ignore;

            VisualElement Fill = new VisualElement();
            Fill.AddToClassList("enemy-bar__fill");
            Fill.pickingMode = PickingMode.Ignore;
            Bar.Add(Fill);

            return new BarEntry { Bar = Bar, Fill = Fill };
        }

        public void Bind(Unit Owner)
        {
            Tracked = Owner;
            Anchor  = Owner.transform;

            // Capture a fixed local height once, so the bar keeps an equal distance above the
            // model and never jitters with animated collider bounds.
            Collider Body = Owner.GetComponentInChildren<Collider>();
            if (Body != null)
            {
                HeadHeight = (Body.bounds.max.y - Anchor.position.y) + HeadPadding;
            }
            else
            {
                HeadHeight = FallbackHeight;
            }
        }

        public void Unbind()
        {
            Tracked = null;
            Anchor  = null;
        }

        // World point a fixed distance above the creature's root, following it smoothly.
        public Vector3 GetAnchorPosition()
        {
            return Anchor.position + Vector3.up * HeadHeight;
        }

        private Unit Tracked;
        private Transform Anchor;
        private float HeadHeight;
    }
}
