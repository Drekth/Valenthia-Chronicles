using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// Shared renderer for a container's item grid: a fixed background grid of slot cells, the
// first ones filled with an item icon (or its name as fallback) and quantity. Used by both
// the loot window and the player inventory window so the grid look stays consistent.
public static class ItemGridView
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    // Rebuilds the grid into Grid: always draws max(SlotCount, Stacks.Count) cells so the
    // empty cells form the background grid. OnSlotClicked may be null for a read-only grid.
    public static void Rebuild(VisualElement Grid, IReadOnlyList<ItemStack> Stacks, int SlotCount, Action<int> OnSlotClicked)
    {
        if (Grid == null)
        {
            return;
        }

        Grid.Clear();

        int CellCount = Mathf.Max(SlotCount, Stacks.Count);
        for (int I = 0; I < CellCount; I++)
        {
            Grid.Add(BuildSlot(Stacks, I, OnSlotClicked));
        }
    }

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private static VisualElement BuildSlot(IReadOnlyList<ItemStack> Stacks, int Index, Action<int> OnSlotClicked)
    {
        VisualElement Slot = new VisualElement();
        Slot.AddToClassList("item-slot");

        // Empty cell: it just paints the grid background.
        if (Index >= Stacks.Count)
        {
            return Slot;
        }

        ItemStack Stack = Stacks[Index];

        if (Stack.Item != null && Stack.Item.Icon != null)
        {
            Slot.style.backgroundImage = new StyleBackground(Stack.Item.Icon);
        }
        else if (Stack.Item != null)
        {
            // Fallback when an item has no icon yet: show its name so the slot stays readable.
            Label Name = new Label(Stack.Item.DisplayName);
            Name.AddToClassList("item-slot__name");
            Name.pickingMode = PickingMode.Ignore;
            Slot.Add(Name);
        }

        Label Count = new Label(Stack.Amount.ToString());
        Count.AddToClassList("item-slot__count");
        Count.pickingMode = PickingMode.Ignore;
        Slot.Add(Count);

        if (OnSlotClicked != null)
        {
            int Captured = Index;
            Slot.RegisterCallback<ClickEvent>(Event => OnSlotClicked(Captured));
        }

        return Slot;
    }
}
