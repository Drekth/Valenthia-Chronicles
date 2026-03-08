using System;

/// <summary>
/// Identifies the type of an entity in the game world.
/// </summary>
public enum TypeId : byte
{
    Object      = 0,
    WorldObject = 1,
    Unit        = 2,
    Player      = 3,
    Creature    = 4,
    GameObject  = 5,
    Item        = 6
}

/// <summary>
/// Bitmask for testing multiple types at once.
/// Each bit corresponds to a TypeId value.
/// </summary>
[Flags]
public enum TypeMask : uint
{
    None        = 0,
    Object      = 1 << TypeId.Object,
    WorldObject = 1 << TypeId.WorldObject,
    Unit        = 1 << TypeId.Unit,
    Player      = 1 << TypeId.Player,
    Creature    = 1 << TypeId.Creature,
    GameObject  = 1 << TypeId.GameObject,
    Item        = 1 << TypeId.Item,

    // Composite masks
    AnyUnit = Unit | Player | Creature,
    AnyWorldObject = WorldObject | AnyUnit | GameObject
}

/// <summary>
/// Determines how a Creature reacts to nearby hostile units.
/// Used by AI decision logic to choose combat behavior.
/// </summary>
public enum ReactState : byte
{
    Passive    = 0,
    Defensive  = 1,
    Aggressive = 2,
    Assist     = 3
}

/// <summary>
/// Bitflag states for a Unit. Drives movement/action restrictions.
/// </summary>
[Flags]
public enum UnitStates : uint
{
    None            = 0,
    Died            = 0x00000001,
    MeleeAttacking  = 0x00000002,
    Charmed         = 0x00000004,
    Stunned         = 0x00000008,
    Roaming         = 0x00000010,
    Chase           = 0x00000020,
    Focusing        = 0x00000040,
    Fleeing         = 0x00000080,
    InFlight        = 0x00000100,
    Follow          = 0x00000200,
    Root            = 0x00000400,
    Confused        = 0x00000800,
    Distracted      = 0x00001000,
    Casting         = 0x00008000,
    Possessed       = 0x00010000,
    Charging        = 0x00020000,
    Jumping         = 0x00040000,
    Move            = 0x00100000,
    Rotating        = 0x00200000,
    Evade           = 0x00400000,

    // Composite masks
    Moving       = Roaming | Chase | Follow | Fleeing | InFlight | Move | Charging | Jumping,
    Controlled   = Stunned | Confused | Fleeing,
    LostControl  = Controlled | Possessed | Jumping | Charging,
    CannotTurn   = LostControl | Rotating | Focusing,
    CannotMove   = Root | Stunned | Died | Distracted
}

/// <summary>
/// Primary unit flags controlling combat and interaction behavior.
/// </summary>
[Flags]
public enum UnitFlags : uint
{
    None                = 0,
    NonAttackable       = 0x00000002,
    PlayerControlled    = 0x00000008,
    ImmuneToPC          = 0x00000100,
    ImmuneToNPC         = 0x00000200,
    Looting             = 0x00000400,
    Pacified            = 0x00020000,
    Stunned             = 0x00040000,
    InCombat            = 0x00080000,
    Disarmed            = 0x00200000,
    Confused            = 0x00400000,
    Fleeing             = 0x00800000,
    Possessed           = 0x01000000,
    Uninteractible      = 0x02000000,
    Mounted             = 0x08000000,
    Immune              = 0x80000000
}

/// <summary>
/// Secondary unit flags for additional state control.
/// </summary>
[Flags]
public enum UnitFlags2 : uint
{
    None                    = 0,
    FeignDeath              = 0x00000001,
    DisarmOffhand           = 0x00000080,
    DisarmRanged            = 0x00000400,
    RegeneratePower         = 0x00000800,
    InteractWhileHostile    = 0x00004000,
    CannotTurn              = 0x00008000,
    Untargetable            = 0x04000000
}
