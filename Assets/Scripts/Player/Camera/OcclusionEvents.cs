// Events du domaine occlusion caméra, publiés/consommés via EventBus<T>.

// Publié quand le joueur pénètre dans le volume intérieur d'un bâtiment. Le toit (et plus tard
// les étages supérieurs) se fond ; d'autres systèmes (audio d'intérieur, météo, ambiance) peuvent
// aussi y réagir. BuildingId permet de distinguer les bâtiments imbriqués/adjacents.
public struct InteriorEnteredEvent : IEvent
{
    public int BuildingId;
}

// Publié quand le joueur quitte le volume intérieur d'un bâtiment : le toit redevient opaque.
public struct InteriorExitedEvent : IEvent
{
    public int BuildingId;
}
