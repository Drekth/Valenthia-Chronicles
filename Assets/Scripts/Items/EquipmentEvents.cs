// Events du domaine équipement, publiés/consommés via EventBus<T>.

// Notifie qu'un changement d'équipement a eu lieu (équiper/déséquiper), pour rafraîchir les visuels
// portés et les fenêtres d'inventaire/équipement (sans payload).
public struct EquipmentChangedEvent : IEvent
{
}
