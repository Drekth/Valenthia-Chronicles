using System;
using System.Collections.Generic;
using UnityEngine;

////////////////////////////////////////////////////////////
/// Loot entry                                           ///
////////////////////////////////////////////////////////////

// One potential drop: an item, a quantity range, and an independent roll chance [0..1].
[Serializable]
public struct LootEntry
{
    public ItemData Item     => DropItem;
    public int      MinCount => MinAmount;
    public int      MaxCount => MaxAmount;
    public float    Chance   => DropChance;

    [SerializeField] private ItemData DropItem;
    [SerializeField] private int MinAmount;
    [SerializeField] private int MaxAmount;
    [Range(0f, 1f)]
    [SerializeField] private float DropChance;
}

////////////////////////////////////////////////////////////
/// Loot table asset                                     ///
////////////////////////////////////////////////////////////

// DT_ ScriptableObject describing a container's possible drops.
[CreateAssetMenu(menuName = "Valenthia/Items/Loot Table")]
public class LootTableData : ScriptableObject
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    // Rolls every entry independently and returns a fresh list of stacks. Pure read of
    // the asset's config — it never mutates this ScriptableObject (shared in builds).
    public List<ItemStack> Roll()
    {
        List<ItemStack> Result = new List<ItemStack>();

        if (Entries == null)
        {
            return Result;
        }

        foreach (LootEntry Entry in Entries)
        {
            if (Entry.Item == null)
            {
                continue;
            }

            if (UnityEngine.Random.value > Entry.Chance)
            {
                continue;
            }

            int Quantity = UnityEngine.Random.Range(Entry.MinCount, Entry.MaxCount + 1);
            if (Quantity <= 0)
            {
                continue;
            }

            Result.Add(new ItemStack(Entry.Item, Quantity));
        }

        return Result;
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [SerializeField] private LootEntry[] Entries;
}
