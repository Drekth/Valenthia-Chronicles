// Events du domaine joueur, publiés/consommés via EventBus<T>.

// Annonce le corps joueur actif : le pawn le publie avec lui-même au spawn et avec null au despawn,
// pour que le PlayerController persistant puisse le posséder à travers les changements de zone.
public struct PlayerSpawnedEvent : IEvent
{
    public PlayerCharacter Character;
}

// Lie (ou vide, si Spell == null) un sort à un slot de l'action bar. Publié par le joueur
// quand son loadout devient actif ; consommé par l'action bar du HUD pour peindre l'icône.
public struct HotbarSlotAssignedEvent : IEvent
{
    public int Slot;
    public SpellData Spell;
}
