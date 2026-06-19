// Events du domaine joueur, publiés/consommés via EventBus<T>.

// Annonce le corps joueur actif : le pawn le publie avec lui-même au spawn et avec null au despawn,
// pour que le PlayerController persistant puisse le posséder à travers les changements de zone.
public struct PlayerSpawnedEvent : IEvent
{
    public PlayerCharacter Character;
}
