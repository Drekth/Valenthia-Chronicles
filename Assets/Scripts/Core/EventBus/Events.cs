// Core marker interface for all EventBus events. Concrete events are declared per module
// (e.g. Player/PlayerEvents.cs, Entities/SelectionEvents.cs) as structs implementing IEvent.
public interface IEvent { }