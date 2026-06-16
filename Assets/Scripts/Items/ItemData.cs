using UnityEngine;

// Immutable definition of an item, authored as a DT_ ScriptableObject asset. Shared by
// every stack that references it — never mutated at runtime.
[CreateAssetMenu(menuName = "Valenthia/Items/Item Data")]
public class ItemData : ScriptableObject
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public string        Id           => ItemId;
    public string        DisplayName  => ItemName;
    public string        Description  => ItemDescription;
    public Sprite        Icon         => ItemIcon;
    public int           MaxStackSize => MaxStack;
    public ItemRarity    Rarity       => RarityTier;
    public ItemType      Type         => ItemCategory;
    public EquipmentSlot Slot         => EquipSlot;
    public bool          IsEquippable => EquipSlot != EquipmentSlot.None;

    // 3D model attached to the player's body when this item is equipped, or null when the
    // item has no worn visual. AttachPosition / AttachEuler are local offsets applied on the
    // body socket so the model lines up with the bone (the imported grip rarely matches it).
    public GameObject WorldModel     => EquippedModel;
    public Vector3    AttachPosition => AttachOffsetPosition;
    public Vector3    AttachEuler    => AttachOffsetEuler;

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
    [SerializeField] private GameObject EquippedModel;
    [SerializeField] private Vector3 AttachOffsetPosition;
    [SerializeField] private Vector3 AttachOffsetEuler;

    [Header("Rules")]
    [SerializeField] private int MaxStack = 99;
    [SerializeField] private ItemRarity RarityTier = ItemRarity.Common;
    [SerializeField] private ItemType ItemCategory = ItemType.Misc;
    [SerializeField] private EquipmentSlot EquipSlot = EquipmentSlot.None;
}
