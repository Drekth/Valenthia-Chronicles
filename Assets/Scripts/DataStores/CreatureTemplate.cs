using UnityEngine;

/// <summary>
/// ScriptableObject template defining a creature's base data.
/// Used by Creature components to initialize identity, behavior, and stats.
/// Drag into a Creature component to define its setup.
/// </summary>
[CreateAssetMenu(fileName = "New Creature Template", menuName = "Valenthia/Creature Template")]
public class CreatureTemplate : ScriptableObject
{
    public string CreatureName => creatureName;
    public CreatureType CreatureType => creatureType;
    public int Level => level;
    public ReactState DefaultReactState => defaultReactState;
    public float WanderRadius => wanderRadius;
    public UnitAttributes.BaseAttributesData BaseAttributes => baseAttributes;
    public UnitAttributes.Regeneration Regeneration => regeneration;
    public UnitFlags DefaultFlags => defaultFlags;

    [Header("Identity")]
    [SerializeField] private string creatureName;
    [SerializeField] private CreatureType creatureType = CreatureType.Humanoid;
    [SerializeField] private int level = 1;

    [Header("Behavior")]
    [SerializeField] private ReactState defaultReactState = ReactState.Aggressive;
    [SerializeField] private float wanderRadius = 5f;

    [Header("Attributes and Stats")]
    [SerializeField] private UnitAttributes.BaseAttributesData baseAttributes;
    [SerializeField] private UnitAttributes.Regeneration regeneration;

    [Header("Flags")]
    [SerializeField] private UnitFlags defaultFlags = UnitFlags.None;
}
