// Events du domaine temps/état de jeu, publiés/consommés via EventBus<T>.

// Levé quand le jeu est mis en pause.
public struct PausedEvent : IEvent
{
}

// Levé quand le jeu reprend après une pause.
public struct ResumedEvent : IEvent
{
}
