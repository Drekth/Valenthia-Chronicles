using System.Collections.Generic;
using UnityEngine;

// The player's worn gear. Pure runtime model: it maps body slots to the equipped item and
// enforces the slot rules (swap when a slot is taken, two interchangeable ring positions for
// the single Ring item slot). It never touches the bag and never raises the changed channel
// itself — callers move items between the Container and these slots and announce the change
// once the whole transaction is done. Lives on the persistent player rig and publishes itself
// through the ServiceLocator so UI and gameplay reach it without scene lookups.
public class Equipment : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Constants                                            ///
    ////////////////////////////////////////////////////////////

    // The doll exposes two interchangeable ring positions for the single Ring item slot.
    public const int RingSlotCount = 2;

    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    // Item worn in a slot, or null. RingIndex selects which ring position (0/1); it is
    // ignored for every other slot.
    public ItemData Get(EquipmentSlot Slot, int RingIndex = 0)
    {
        if (Slot == EquipmentSlot.Ring)
        {
            return IsValidRingIndex(RingIndex) ? Rings[RingIndex] : null;
        }

        return Worn.TryGetValue(Slot, out ItemData Item) ? Item : null;
    }

    // Equips an item into its declared slot. A ring fills the first free position, otherwise
    // it replaces the first one. Whatever was already in the target slot is returned in
    // Displaced so the caller can put it back in the bag. Returns false for a non-equippable
    // item (Slot == None).
    public bool TryEquip(ItemData Item, out ItemData Displaced)
    {
        Displaced = null;

        if (Item == null || !Item.IsEquippable)
        {
            return false;
        }

        EquipmentSlot Slot = Item.Slot;

        if (Slot == EquipmentSlot.Ring)
        {
            int Index = FirstFreeRing();
            Displaced = Rings[Index];
            Rings[Index] = Item;
            return true;
        }

        Worn.TryGetValue(Slot, out Displaced);
        Worn[Slot] = Item;
        return true;
    }

    // Removes the item worn in a slot and returns it in Removed. Returns false when the slot
    // is already empty.
    public bool TryUnequip(EquipmentSlot Slot, int RingIndex, out ItemData Removed)
    {
        Removed = Get(Slot, RingIndex);

        if (Removed == null)
        {
            return false;
        }

        if (Slot == EquipmentSlot.Ring)
        {
            Rings[RingIndex] = null;
        }
        else
        {
            Worn.Remove(Slot);
        }

        return true;
    }

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void Awake()
    {
        ServiceLocator.Register<Equipment>(this);
    }

    private void OnDestroy()
    {
        if (ServiceLocator.TryGet<Equipment>(out Equipment Current) && Current == this)
        {
            ServiceLocator.Unregister<Equipment>();
        }
    }

    // First empty ring position, or 0 when both are taken (the first one is replaced).
    private int FirstFreeRing()
    {
        for (int I = 0; I < RingSlotCount; I++)
        {
            if (Rings[I] == null)
            {
                return I;
            }
        }

        return 0;
    }

    private bool IsValidRingIndex(int RingIndex)
    {
        return RingIndex >= 0 && RingIndex < RingSlotCount;
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    private readonly Dictionary<EquipmentSlot, ItemData> Worn = new Dictionary<EquipmentSlot, ItemData>();
    private readonly ItemData[] Rings = new ItemData[RingSlotCount];
}
