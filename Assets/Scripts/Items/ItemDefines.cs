using System;
using UnityEngine;

////////////////////////////////////////////////////////////
/// Item enums                                           ///
////////////////////////////////////////////////////////////

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public enum ItemType
{
    Misc,
    Weapon,
    Armor,
    Consumable,
    Quest,
    Currency
}

////////////////////////////////////////////////////////////
/// Item stack — runtime quantity wrapper                ///
////////////////////////////////////////////////////////////

// Pairs an item definition with a quantity. ItemData stays the immutable definition;
// the mutable amount lives here, never on the shared ScriptableObject. Immutable by
// design — callers replace a stack rather than edit it in place.
[Serializable]
public struct ItemStack
{
    public ItemStack(ItemData Item, int Amount)
    {
        StackItem = Item;
        StackAmount = Amount;
    }

    public ItemData Item   => StackItem;
    public int      Amount => StackAmount;

    [SerializeField] private ItemData StackItem;
    [SerializeField] private int StackAmount;
}
