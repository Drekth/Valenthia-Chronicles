using System;
using UnityEngine;

/// <summary>
/// An entity that exists positioned in the game world.
/// Provides spatial utilities (distance, range checks).
/// </summary>
public abstract class WorldObject : MonoBehaviour
{
    /// <summary>
    /// Type identifier for the entity hierarchy.
    /// Can be overridden by derived classes.
    /// </summary>
    public virtual TypeId ObjectTypeId => TypeId.WorldObject;

    /// <summary>
    /// Safe downcast — returns null if this entity is not of type T.
    /// </summary>
    public T As<T>() where T : WorldObject => this as T;

    /// <summary>
    /// Shorthand for transform.position.
    /// </summary>
    public Vector3 Position
    {
        get => transform.position;
        set => transform.position = value;
    }

    /// <summary>
    /// Shorthand for transform.rotation.
    /// </summary>
    public Quaternion Orientation
    {
        get => transform.rotation;
        set => transform.rotation = value;
    }

    /// <summary>
    /// Calculates the distance between this object and another WorldObject.
    /// </summary>
    public float GetDistanceTo(WorldObject other)
    {
        return Vector3.Distance(Position, other.Position);
    }

    /// <summary>
    /// Checks if another WorldObject is within a given range.
    /// Uses squared distance for performance.
    /// </summary>
    public bool IsWithinRange(WorldObject target, float range)
    {
        return (Position - target.Position).sqrMagnitude <= range * range;
    }

    /// <summary>
    /// Calculates a point at a given distance and angle from this object.
    /// Useful for spawn positions, ability targeting, etc.
    /// </summary>
    public Vector3 GetNearPoint(float distance, float angle)
    {
        float rad = angle * Mathf.Deg2Rad;
        return Position + new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad)) * distance;
    }
}
