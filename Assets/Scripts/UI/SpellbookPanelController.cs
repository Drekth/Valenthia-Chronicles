using UnityEngine;
using UnityEngine.UIElements;

// Openable spellbook (grimoire), toggled by a key (see ToggleBinding). Master-detail layout: the
// left list holds every spell the player knows (PlayerSpellbook), clicking one shows its cost,
// cooldown, range and description on the right. Window chrome and Show/Hide/toggle lifecycle come
// from UIWindowController; the known set is read from the ServiceLocator on each open.
//
// A spell entry can be dragged onto the HUD action bar to bind it to a slot. The drag is local to
// this window — it shows a ghost that follows the cursor and, on release, publishes a
// SpellDragDroppedEvent with the panel-space pointer position. The HUD action bar (which owns its
// own slot geometry) decides whether the drop landed on a slot. A press that does not move past a
// small threshold is treated as a click and selects the spell instead.
public class SpellbookPanelController : UIWindowController
{
    ////////////////////////////////////////////////////////////
    /// Constants                                            ///
    ////////////////////////////////////////////////////////////

    private const string EntryClass         = "spellbook__entry";
    private const string EntrySelectedClass = "spellbook__entry--selected";
    private const string EntryIconClass     = "spellbook__entry-icon";
    private const string EntryNameClass     = "spellbook__entry-name";

    // Pointer travel (panel pixels) past which a press becomes a drag rather than a click.
    private const float DragThreshold = 6.0f;

    // Half the ghost size, used to centre it on the cursor (must match the .uss width/height).
    private const float GhostHalfSize = 24.0f;

    ////////////////////////////////////////////////////////////
    /// Protected                                            ///
    ////////////////////////////////////////////////////////////

    protected override void OnBindContent(VisualElement Root)
    {
        SpellbookRoot     = Root;
        ListContainer     = Root.Q<VisualElement>("SpellList");
        EmptyHint         = Root.Q<Label>("EmptyHint");
        DetailScroll      = Root.Q<VisualElement>("SpellDetailScroll");
        DetailIcon        = Root.Q<VisualElement>("DetailIcon");
        DetailName        = Root.Q<Label>("DetailName");
        DetailManaCost    = Root.Q<Label>("DetailManaCost");
        DetailCooldown    = Root.Q<Label>("DetailCooldown");
        DetailRange       = Root.Q<Label>("DetailRange");
        DetailDescription = Root.Q<Label>("DetailDescription");
        DragGhost         = Root.Q<VisualElement>("DragGhost");
    }

    protected override void Rebuild()
    {
        if (ListContainer == null)
        {
            return;
        }

        ListContainer.Clear();

        PlayerSpellbook Book = ResolveSpellbook();
        SpellData First = null;
        SpellData Match = null;

        if (Book != null)
        {
            foreach (SpellData Spell in Book.KnownSpells)
            {
                if (Spell == null)
                {
                    continue;
                }

                AddEntry(Spell);

                if (First == null)
                {
                    First = Spell;
                }

                if (Spell.Id == SelectedSpellId)
                {
                    Match = Spell;
                }
            }
        }

        bool HasSpells = ListContainer.childCount > 0;
        SetDisplay(EmptyHint, !HasSpells);
        SetDisplay(DetailScroll, HasSpells);

        if (!HasSpells)
        {
            SelectedSpellId = null;
            return;
        }

        // Keep the previous selection when it is still known, otherwise fall back to the first spell.
        SelectSpell(Match != null ? Match : First);
    }

    // Cancel any in-flight drag if the window is closed mid-gesture.
    protected override void OnHidden()
    {
        CancelDrag();
    }

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void AddEntry(SpellData Spell)
    {
        VisualElement Entry = new VisualElement();
        Entry.AddToClassList(EntryClass);
        Entry.userData = Spell;

        VisualElement Icon = new VisualElement();
        Icon.AddToClassList(EntryIconClass);
        if (Spell.Icon != null)
        {
            Icon.style.backgroundImage = new StyleBackground(Spell.Icon);
        }
        Entry.Add(Icon);

        Label Name = new Label(Spell.DisplayName);
        Name.AddToClassList(EntryNameClass);
        Entry.Add(Name);

        Entry.RegisterCallback<PointerDownEvent>(OnEntryPointerDown);
        Entry.RegisterCallback<PointerMoveEvent>(OnEntryPointerMove);
        Entry.RegisterCallback<PointerUpEvent>(OnEntryPointerUp);

        ListContainer.Add(Entry);
    }

    // Start tracking a press: capture the pointer so this entry keeps receiving move/up even when
    // the cursor leaves it, and stop propagation so the surrounding ScrollView does not drag-scroll.
    private void OnEntryPointerDown(PointerDownEvent Event)
    {
        VisualElement Entry = Event.currentTarget as VisualElement;
        if (Entry == null || !(Entry.userData is SpellData Spell))
        {
            return;
        }

        DownEntry     = Entry;
        DownSpell     = Spell;
        DragStart     = Event.position;
        DragPointerId = Event.pointerId;
        Dragging      = false;

        Entry.CapturePointer(Event.pointerId);
        Event.StopPropagation();
    }

    private void OnEntryPointerMove(PointerMoveEvent Event)
    {
        if (DownEntry == null || Event.pointerId != DragPointerId)
        {
            return;
        }

        if (!Dragging)
        {
            Vector2 Delta = (Vector2)Event.position - DragStart;
            if (Delta.sqrMagnitude < DragThreshold * DragThreshold)
            {
                return;
            }

            BeginDrag(DownSpell);
        }

        MoveGhost(Event.position);
    }

    private void OnEntryPointerUp(PointerUpEvent Event)
    {
        if (DownEntry == null || Event.pointerId != DragPointerId)
        {
            return;
        }

        VisualElement Entry = DownEntry;
        if (Entry.HasPointerCapture(DragPointerId))
        {
            Entry.ReleasePointer(DragPointerId);
        }

        if (Dragging)
        {
            HideGhost();
            EventBus<SpellDragDroppedEvent>.Raise(new SpellDragDroppedEvent
            {
                Spell         = DownSpell,
                PanelPosition = Event.position,
            });
        }
        else
        {
            // A press that never moved is a plain click: select the spell.
            SelectSpell(DownSpell);
        }

        ResetDrag();
    }

    private void BeginDrag(SpellData Spell)
    {
        Dragging = true;

        if (DragGhost == null)
        {
            return;
        }

        DragGhost.style.backgroundImage = Spell.Icon != null
            ? new StyleBackground(Spell.Icon)
            : new StyleBackground();
        DragGhost.style.display = DisplayStyle.Flex;
    }

    private void MoveGhost(Vector2 PanelPosition)
    {
        if (DragGhost == null || DragGhost.parent == null)
        {
            return;
        }

        Vector2 Local = DragGhost.parent.WorldToLocal(PanelPosition);
        DragGhost.style.left = Local.x - GhostHalfSize;
        DragGhost.style.top  = Local.y - GhostHalfSize;
    }

    private void HideGhost()
    {
        if (DragGhost != null)
        {
            DragGhost.style.display = DisplayStyle.None;
        }
    }

    // Releases capture and clears drag state without raising a drop (e.g. window closed mid-drag).
    private void CancelDrag()
    {
        if (DownEntry != null && DownEntry.HasPointerCapture(DragPointerId))
        {
            DownEntry.ReleasePointer(DragPointerId);
        }

        HideGhost();
        ResetDrag();
    }

    private void ResetDrag()
    {
        DownEntry     = null;
        DownSpell     = null;
        Dragging      = false;
        DragPointerId = -1;
    }

    // Highlights the selected list entry and fills the detail pane from the spell definition.
    private void SelectSpell(SpellData Spell)
    {
        if (Spell == null)
        {
            return;
        }

        SelectedSpellId = Spell.Id;

        for (int Index = 0; Index < ListContainer.childCount; Index++)
        {
            VisualElement Entry = ListContainer[Index];
            bool IsSelected = Entry.userData is SpellData EntrySpell && EntrySpell.Id == Spell.Id;
            Entry.EnableInClassList(EntrySelectedClass, IsSelected);
        }

        ShowDetail(Spell);
    }

    private void ShowDetail(SpellData Spell)
    {
        if (DetailIcon != null)
        {
            DetailIcon.style.backgroundImage = Spell.Icon != null
                ? new StyleBackground(Spell.Icon)
                : new StyleBackground();
        }

        DetailName.text        = Spell.DisplayName;
        DetailManaCost.text    = $"Coût : {Spell.ManaCost:0} mana";
        DetailCooldown.text    = $"Temps de recharge : {Spell.Cooldown:0.##} s";
        DetailRange.text       = $"Portée : {Spell.Range:0.##} m";
        DetailDescription.text = Spell.Description;
    }

    private static PlayerSpellbook ResolveSpellbook()
    {
        ServiceLocator.TryGet<PlayerSpellbook>(out PlayerSpellbook Book);
        return Book;
    }

    private static void SetDisplay(VisualElement Element, bool Visible)
    {
        if (Element != null)
        {
            Element.style.display = Visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    private VisualElement SpellbookRoot;
    private VisualElement ListContainer;
    private Label         EmptyHint;
    private VisualElement DetailScroll;
    private VisualElement DetailIcon;
    private Label         DetailName;
    private Label         DetailManaCost;
    private Label         DetailCooldown;
    private Label         DetailRange;
    private Label         DetailDescription;
    private VisualElement DragGhost;

    private string SelectedSpellId;

    private VisualElement DownEntry;
    private SpellData     DownSpell;
    private Vector2       DragStart;
    private int           DragPointerId = -1;
    private bool          Dragging;
}
