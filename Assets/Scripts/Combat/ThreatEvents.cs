// Threat-domain events, published/consumed via EventBus<T>. Owned by the threat module.

// Raised by a creature's ThreatManager when its selected victim changes (initial acquisition,
// threat swap, taunt, fixate, or loss of all targets), so AI feedback and a future threat UI can
// react without polling. NewVictim is null when the creature drops all of its targets.
public struct VictimChangedEvent : IEvent
{
    public Unit Creature;
    public Unit OldVictim;
    public Unit NewVictim;
}

// Raised by ThreatManager.AddThreat whenever an attacker's accumulated threat changes. A seam for a
// future threat-meter UI; no consumer yet.
public struct ThreatChangedEvent : IEvent
{
    public Unit Creature;
    public Unit Attacker;
    public float NewThreat;
}
