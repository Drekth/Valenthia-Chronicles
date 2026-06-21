// Creature AI definitions: how a creature is disposed toward the player and what combat phase
// its reactive brain is currently in. WoW/TrinityCore-inspired, kept minimal.

// Disposition of a creature toward the player (TrinityCore-style react state).
public enum ReactState
{
    Friendly,   // Cannot be attacked by the player; never initiates combat.
    Neutral,    // Retaliates only once attacked (defensive).
    Aggressive, // Attacks the player on sight within its aggro radius.
}

// Runtime combat phase of a creature brain.
public enum CombatState
{
    Idle,    // Wandering, no target.
    Combat,  // Chasing / attacking CurrentTarget.
}
