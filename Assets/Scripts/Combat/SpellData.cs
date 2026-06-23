using System.Collections.Generic;
using UnityEngine;

// Immutable definition of a spell, authored as a DT_ ScriptableObject asset. Shared by every
// caster that references it — never mutated at runtime. The effect list is polymorphic
// ([SerializeReference]): Unity 6's managed-reference picker adds concrete effects in the
// Inspector without a custom drawer.
[CreateAssetMenu(menuName = "Valenthia/Combat/Spell")]
public class SpellData : ScriptableObject
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public string             Id          => SpellId;
    public string             DisplayName => SpellName;
    public string             Description => SpellDescription;
    public Sprite             Icon        => SpellIcon;
    public float              Range       => CastRange;
    public float              Cooldown    => CooldownSeconds;
    public float              ManaCost    => ManaCostAmount;
    public List<SpellEffect>  Effects     => SpellEffects;

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [Header("Identity")]
    [SerializeField] private string SpellId;
    [SerializeField] private string SpellName;
    [SerializeField, TextArea] private string SpellDescription;
    [SerializeField] private Sprite SpellIcon;

    [Header("Rules")]
    // Max XZ distance from caster to target. Cooldown doubles as attack speed for the basic
    // attack until a dedicated attack-speed stat exists.
    [SerializeField] private float CastRange = 3.0f;
    [SerializeField] private float CooldownSeconds = 1.5f;
    [SerializeField] private float ManaCostAmount = 0.0f;

    [Header("Effects")]
    [SerializeReference] private List<SpellEffect> SpellEffects = new List<SpellEffect>();
}
