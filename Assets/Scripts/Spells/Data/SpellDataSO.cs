using UnityEngine;
using VInspector;


[CreateAssetMenu(fileName = "New Spell", menuName = "Valenthia/Spells/Spell Data")]
public class SpellDataSO : ScriptableObject
{
    public string SpellName => spellName;
    public string Description => description;
    public Sprite Icon => icon;
    public CastType CastType => castType;
    public float CastTime => castTime;
    public float Cooldown => cooldown;
    public ResourceType ResourceType => resourceType;
    public int ResourceCost => resourceCost;
    public float Range => range;
    public TargetType TargetType => targetType;
    public DamageType DamageType => damageType;
    public int Damage => damage;
    public GameObject ImpactVFXPrefab => impactVFXPrefab;
    public float VFXDuration => vfxDuration;

    public bool IsInstant => castType == CastType.Instant;
    public bool HasCooldown => cooldown > 0f;
    public bool HasResourceCost => resourceCost > 0;
    public bool HasVFX => impactVFXPrefab != null;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(spellName))
            spellName = name;

        if (castTime < 0f)
            castTime = 0f;

        if (cooldown < 0f)
            cooldown = 0f;

        if (range < 0f)
            range = 0f;

        if (resourceCost < 0)
            resourceCost = 0;

        if (damage < 0)
            damage = 0;

        if (vfxDuration < 0f)
            vfxDuration = 0f;
    }

    [Header("Basic Information")]
    [SerializeField] private string spellName;
    [SerializeField, TextArea(2, 4)] private string description;
    [SerializeField] private Sprite icon;

    [Header("Cast Settings")]
    [SerializeField] private CastType castType = CastType.Instant;
    [SerializeField] private float castTime;
    [SerializeField] private float cooldown;

    [Header("Resource")]
    [SerializeField] private ResourceType resourceType = ResourceType.Mana;
    [SerializeField] private int resourceCost;

    [Header("Targeting")]
    [SerializeField] private TargetType targetType = TargetType.Enemy;
    [HideIf("targetType", TargetType.Self)]
    [SerializeField] private float range = 30f;
    [EndIf]

    [Header("Damage")]
    [SerializeField] private DamageType damageType = DamageType.Magical;
    [SerializeField] private int damage;

    [Header("Visual Effects")]
    [SerializeField] private GameObject impactVFXPrefab;
    [SerializeField] private float vfxDuration = 2f;
}
