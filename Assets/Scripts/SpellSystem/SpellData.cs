using UnityEngine;
using VInspector;

namespace SpellSystem
{
    [CreateAssetMenu(fileName = "New Spell", menuName = "Spell System/Spell Data")]
    public class SpellData : ScriptableObject
    {
        [Header("Basic Information")]
        [SerializeField] private string spellName;
        [SerializeField, TextArea(2, 4)] private string description;
        [SerializeField] private Sprite icon;

        [Header("Cast Settings")]
        [SerializeField] private CastType castType = CastType.Instant;
        [SerializeField] private float castTime;
        [SerializeField] private float cooldown;
        [SerializeField] private int manaCost;

        [Header("Targeting")]
        [SerializeField] private TargetType targetType = TargetType.Enemy;
        [HideIf("targetType", TargetType.Self)]
        [SerializeField] private float range = 30f;
        [EndIf]
        
        [Header("Damage")]
        [SerializeField] private int damage;

        [Header("Visual Effects")]
        [SerializeField] private GameObject impactVFXPrefab;
        [SerializeField] private float vfxDuration = 2f;

        public string SpellName => spellName;
        public string Description => description;
        public Sprite Icon => icon;
        public CastType CastType => castType;
        public float CastTime => castTime;
        public float Cooldown => cooldown;
        public int ManaCost => manaCost;
        public float Range => range;
        public TargetType TargetType => targetType;
        public int Damage => damage;
        public GameObject ImpactVFXPrefab => impactVFXPrefab;
        public float VFXDuration => vfxDuration;

        public bool IsInstant => castType == CastType.Instant;
        public bool HasCooldown => cooldown > 0f;
        public bool HasManaCost => manaCost > 0;
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

            if (manaCost < 0)
                manaCost = 0;

            if (damage < 0)
                damage = 0;

            if (vfxDuration < 0f)
                vfxDuration = 0f;
        }
    }
}
