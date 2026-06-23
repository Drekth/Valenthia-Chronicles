using UnityEngine;

[CreateAssetMenu(menuName = "Valenthia/Creatures/Creature Data")]
public class CreatureData : ScriptableObject
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public GameObject      Model   => ModelPrefab;
    public StatData Stats   => StatProfileAsset;
    public float           Speed   => MoveSpeed;
    public float           Radius  => WanderRadius;
    public float           MinIdle => IdleTimeMin;
    public float           MaxIdle => IdleTimeMax;

    public ReactState Reaction       => Disposition;
    public float      AggroRadius    => AggroRange;
    public float      LeashRadius    => LeashRange;
    public float      AttackRange    => MeleeRange;
    public float      AttackDamage   => MeleeDamage;
    public float      AttackCooldown => MeleeCooldown;

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [Header("Visual")]
    [SerializeField] private GameObject ModelPrefab;

    [Header("Stats")]
    // Single source of truth for this creature's stats (health/mana via Stamina/Intellect).
    // Pushed into the StatSheet at spawn by CreatureMotion.
    [SerializeField] private StatData StatProfileAsset;

    [Header("Movement")]
    [SerializeField] private float MoveSpeed    = 3.5f;
    [SerializeField] private float WanderRadius = 10.0f;
    [SerializeField] private float IdleTimeMin  = 2.0f;
    [SerializeField] private float IdleTimeMax  = 5.0f;

    [Header("Combat")]
    [SerializeField] private ReactState Disposition   = ReactState.Neutral;
    [SerializeField] private float      AggroRange    = 8.0f;
    [SerializeField] private float      LeashRange    = 15.0f;
    [SerializeField] private float      MeleeRange    = 2.0f;
    [SerializeField] private float      MeleeDamage   = 5.0f;
    [SerializeField] private float      MeleeCooldown = 2.0f;
}
