using UnityEngine;

// Runtime stat component carried alongside Unit (composition — Unit owns combat state, this owns
// the numbers). v1 is minimal: it reads the immutable StatData and computes the derived pools
// (MaxHealth, MaxMana) from the primaries. There is no per-frame work, and nothing mutates the
// stats at runtime yet.
//
// The data is serialized for unique beings (the player) and overridden at runtime for creatures,
// whose data is sourced from their CreatureData so creature stats have a single source.
//
// Seam for later: a flat bonus layer (gear, auras, ...) will add on top of the base. Get is written
// so that becomes "base + bonus" in a single place, without touching callers. The richer modifier
// pipeline (percent / multiplicative, removal by source) is intentionally out of scope for v1.
public class StatComponent : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Constants                                            ///
    ////////////////////////////////////////////////////////////

    // Placeholder derivation coefficients — to be decided. Promote to a CFG_ ScriptableObject if
    // inspector tuning is ever needed. Kept here so the stat math lives in one place.
    private const float HealthPerStamina = 10.0f;
    private const float ManaPerIntellect = 10.0f;

    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    // Final value of a stat. Primary attributes come straight from the data; derived pools are
    // computed from the primaries. (Future: + accumulated flat bonuses.)
    public float Get(StatType Stat)
    {
        if (Data == null)
        {
            return 0.0f;
        }

        switch (Stat)
        {
            case StatType.MaxHealth: return Data.GetBase(StatType.Stamina)   * HealthPerStamina;
            case StatType.MaxMana:   return Data.GetBase(StatType.Intellect) * ManaPerIntellect;
            default:                 return Data.GetBase(Stat);
        }
    }

    // Overrides the authored data at runtime. Used by creatures, whose data is sourced from their
    // CreatureData rather than serialized on the shared base prefab. Called from
    // CreatureMotion.Awake, before Unit.Start reads the derived maxima.
    public void SetData(StatData NewData)
    {
        Data = NewData;
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [Header("Definition")]
    [SerializeField] private StatData Data;
}
