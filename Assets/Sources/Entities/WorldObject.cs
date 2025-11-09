using UnityEngine;

namespace Sources.Entities
{
    public enum ObjectType
    {
        None,
        NPC,
        GameObject,
        Item,
        Door
    }
    
    /// <summary>
    /// Base class for all interactive objects in the game world.
    /// Inherit from this class to create NPCs, items, doors, and other interactive entities.
    /// </summary>
    public abstract class WorldObject : MonoBehaviour
    {
        [Header("Object Configuration")]
        [SerializeField] private ObjectType objectType = ObjectType.None;

        /// <summary>
        /// Gets the type of this world object.
        /// </summary>
        public ObjectType Type => objectType;

        /// <summary>
        /// Determines if this object can currently be interacted with.
        /// Override this property to add custom interaction conditions.
        /// </summary>
        public virtual bool CanInteract => true;

        /// <summary>
        /// Returns information about this object for debugging or UI purposes.
        /// Override this method to provide custom information.
        /// </summary>
        public virtual string GetInteractionInfo()
        {
            return $"{objectType} - {gameObject.name}";
        }
    }
}
