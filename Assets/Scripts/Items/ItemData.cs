using UnityEngine;

// Immutable definition of an item, authored as a DT_ ScriptableObject asset. Shared by
// every stack that references it — never mutated at runtime.
[CreateAssetMenu(menuName = "Valenthia/Items/Item Data")]
public class ItemData : ScriptableObject
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public string     Id           => ItemId;
    public string     DisplayName  => ItemName;
    public string     Description  => ItemDescription;
    public Sprite     Icon         => ItemIcon;
    public int        MaxStackSize => MaxStack;
    public ItemRarity Rarity       => RarityTier;
    public ItemType   Type         => ItemCategory;

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [Header("Identity")]
    [SerializeField] private string ItemId;
    [SerializeField] private string ItemName;
    [TextArea]
    [SerializeField] private string ItemDescription;

    [Header("Visual")]
    [SerializeField] private Sprite ItemIcon;

    [Header("Rules")]
    [SerializeField] private int MaxStack = 99;
    [SerializeField] private ItemRarity RarityTier = ItemRarity.Common;
    [SerializeField] private ItemType ItemCategory = ItemType.Misc;
}
