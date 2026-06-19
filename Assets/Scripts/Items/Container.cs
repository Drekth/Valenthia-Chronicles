using System.Collections.Generic;
using UnityEngine;

// A bag of item stacks. Doubles as the world lootable (rolls a loot table on first open
// and announces itself through the opened channel) and as the player's inventory (no
// table, filled by transfers). Pure composition — state lives in a runtime list, never
// in a ScriptableObject.
public class Container : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public IReadOnlyList<ItemStack> Items => Contents;

    // Number of slot cells this container exposes — used by the UI to draw the grid.
    public int SlotCount => Capacity;

    // Opens the container: lazily rolls its loot table the first time, then raises the
    // opened channel so the loot window can display it. No roll when used as a bag.
    public void Open()
    {
        if (LootTable != null && !HasRolled)
        {
            Contents.AddRange(LootTable.Roll());
            HasRolled = true;
        }

        EventBus<ContainerOpenedEvent>.Raise(new ContainerOpenedEvent { Container = this });
    }

    // Adds a stack, merging into existing stacks of the same item up to their max size,
    // then spilling the remainder into new slots while capacity allows. Returns true when
    // the whole stack fit.
    public bool TryAdd(ItemStack Stack)
    {
        if (Stack.Item == null || Stack.Amount <= 0)
        {
            return false;
        }

        int Remaining = Stack.Amount;
        int MaxSize = Mathf.Max(1, Stack.Item.MaxStackSize);

        for (int I = 0; I < Contents.Count && Remaining > 0; I++)
        {
            if (Contents[I].Item != Stack.Item)
            {
                continue;
            }

            int Space = MaxSize - Contents[I].Amount;
            if (Space <= 0)
            {
                continue;
            }

            int Moved = Mathf.Min(Space, Remaining);
            Contents[I] = new ItemStack(Stack.Item, Contents[I].Amount + Moved);
            Remaining -= Moved;
        }

        while (Remaining > 0 && Contents.Count < Capacity)
        {
            int Moved = Mathf.Min(MaxSize, Remaining);
            Contents.Add(new ItemStack(Stack.Item, Moved));
            Remaining -= Moved;
        }

        return Remaining == 0;
    }

    // Removes and returns the stack at the given index, or an empty stack if invalid.
    public ItemStack TakeAt(int Index)
    {
        if (Index < 0 || Index >= Contents.Count)
        {
            return default;
        }

        ItemStack Stack = Contents[Index];
        Contents.RemoveAt(Index);
        return Stack;
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [Header("Loot")]
    [SerializeField] private LootTableData LootTable;
    [SerializeField] private int Capacity = 16;

    private readonly List<ItemStack> Contents = new List<ItemStack>();
    private bool HasRolled;
}
