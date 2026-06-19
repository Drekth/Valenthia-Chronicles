// Events du domaine chargement de scènes, publiés/consommés via EventBus<T>.

// Levé au début d'un changement de zone (ex. afficher l'écran de chargement).
public struct LoadStartEvent : IEvent
{
}

// Levé à la fin d'un changement de zone (ex. masquer l'écran de chargement).
public struct LoadCompleteEvent : IEvent
{
}
