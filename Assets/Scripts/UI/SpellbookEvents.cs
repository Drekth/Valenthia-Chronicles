using UnityEngine;

// UI events raised by the spellbook (grimoire) window, published/consumed via EventBus<T>.

// A spell was dropped at the end of a drag from the spellbook, carrying the panel-space position
// of the pointer. Consumed by the HUD action bar, which tests whether the position falls on one of
// its slots and, if so, requests the assignment. Panel coordinates are shared by every UIDocument
// because they all use the same PanelSettings, so this position is directly comparable to slot
// worldBounds.
public struct SpellDragDroppedEvent : IEvent
{
    public SpellData Spell;
    public Vector2 PanelPosition;
}
