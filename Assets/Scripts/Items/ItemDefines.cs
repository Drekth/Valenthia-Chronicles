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

// Body slot an item occupies when equipped. None marks a non-equippable item (potion,
// currency...). A ring targets the single Ring value; the Equipment component routes it
// to one of its two physical ring positions.
public enum EquipmentSlot
{
    None = 0,
    Head,
    Shoulder,
    Chest,
    Hands,
    Legs,
    MainHand,
    OffHand,
    Ring,
    Neck
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
