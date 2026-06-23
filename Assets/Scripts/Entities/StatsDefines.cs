// Stat system definitions. v1 is intentionally minimal: two primary attributes (Stamina,
// Intellect) and the two derived pools they feed (MaxHealth, MaxMana). This enum is the single
// place new stats are declared; the derivation formulas live in StatRules. A bonus layer (flat
// bonuses from gear, auras, ...) is deliberately deferred — see StatSheet.
public enum StatType
{
    // Primary attributes — authored on the StatProfile.
    Stamina,    // → MaxHealth
    Intellect,  // → MaxMana

    // Derived pools — computed from the primaries by StatRules, never authored directly.
    MaxHealth,
    MaxMana,
}
