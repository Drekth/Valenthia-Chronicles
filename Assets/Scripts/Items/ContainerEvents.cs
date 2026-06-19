// Events du domaine conteneurs, publiés/consommés via EventBus<T>.

// Porte le conteneur que le joueur vient d'ouvrir, pour que la fenêtre de loot affiche son contenu
// sans référencer l'objet monde directement.
public struct ContainerOpenedEvent : IEvent
{
    public Container Container;
}

// Notifie la fermeture de la fenêtre de loot (sans payload).
public struct ContainerClosedEvent : IEvent
{
}
