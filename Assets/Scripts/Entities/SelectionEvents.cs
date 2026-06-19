// Events du domaine sélection/entités, publiés/consommés via EventBus<T>.

// Porte la cible courante du joueur (null quand désélectionnée) pour que le halo au sol réagisse
// sans référencer la logique de sélection.
public struct TargetChangedEvent : IEvent
{
    public Unit Target;
}
