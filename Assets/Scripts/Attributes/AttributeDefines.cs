/// <summary>
/// Defines the types of attributes a Unit can have.
/// Primary attributes drive derived combat values.
/// </summary>
public enum AttributeType
{
    // Primary
    Strength,
    Agility,
    Stamina,
    Intellect,
    Spirit,

    // Derived combat
    Armor,
    AttackPower,
    SpellPower,
    CritChance,
    HasteRating,

    // Defensive
    DodgeChance,
    ParryChance,
    BlockChance,

    Count
}

/// <summary>
/// Defines the power resource pools a Unit can have.
/// </summary>
public enum PowerType
{
    Health,
    Mana,
    Stamina,
    Energy,
    Rage,

    Count
}
