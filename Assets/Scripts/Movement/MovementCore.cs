using UnityEngine;

public enum MovementType
{
    None,
    Wander
}

public interface IMovementData
{
    MovementType Type { get; }
}

public interface IMovementProcessor
{
    MovementType HandledType { get; }
    void Process(MovementAuthoring entity, float deltaTime);
}
