using UnityEngine;

// Immutable base stats of a being (player or creature), authored as a DT_ asset. Holds only the
// primary attributes; the derived pools (MaxHealth, MaxMana) are computed at runtime by
// StatComponent from these values. Shared and never mutated at runtime — same Definition/State
// split as SpellData / ItemData.
[CreateAssetMenu(menuName = "Valenthia/Stats/Stat Data")]
public class StatData : ScriptableObject
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public float Stamina   => BaseStamina;
    public float Intellect => BaseIntellect;

    // Base value of a primary attribute by type. Derived stats (MaxHealth, MaxMana) are never
    // authored, so they resolve to 0 here — they are computed by StatComponent instead.
    public float GetBase(StatType Stat)
    {
        switch (Stat)
        {
            case StatType.Stamina:   return BaseStamina;
            case StatType.Intellect: return BaseIntellect;
            default:                 return 0.0f;
        }
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [Header("Primary Attributes")]
    [SerializeField] private float BaseStamina   = 10.0f;
    [SerializeField] private float BaseIntellect = 10.0f;
}
