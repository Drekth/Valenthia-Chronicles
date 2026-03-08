using System;
using UnityEngine;


/// <summary>
/// A non-player Unit in the game world (enemy, friendly NPC, beast, etc.).
/// Loads its initial data from a CreatureTemplate ScriptableObject.
/// Handles runtime setup for AI-driven creatures.
/// </summary>
public class Creature : Unit
{
    public override TypeId ObjectTypeId => TypeId.Creature;
    public CreatureType CreatureType { get; private set; }
    public ReactState CurrentReactState { get; set; }
    public int Level { get; private set; }
    public float WanderRadius { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        if (template != null)
            LoadFromTemplate(template);
    }

    /// <summary>
    /// Initializes this creature from a template ScriptableObject.
    /// Can be called at runtime for dynamically spawned creatures.
    /// </summary>
    public void LoadFromTemplate(CreatureTemplate data)
    {
        CreatureType = data.CreatureType;
        CurrentReactState = data.DefaultReactState;
        Level = data.Level;
        WanderRadius = data.WanderRadius;

        if (!string.IsNullOrEmpty(data.CreatureName))
            gameObject.name = data.CreatureName;

        // Apply default flags
        if (data.DefaultFlags != UnitFlags.None)
            SetFlag(data.DefaultFlags);

        // Load base attributes into UnitAttributes component
        if (Attributes != null)
        {
            Attributes.SetBaseAttributes(data.BaseAttributes);
            Attributes.SetRegeneration(data.Regeneration);
        }
    }

    [Header("Creature Template")]
    [SerializeField] private CreatureTemplate template;
}
