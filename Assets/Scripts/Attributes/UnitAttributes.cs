using System;
using UnityEngine;
using VInspector;

/// <summary>
/// Manages all attributes and power pools for a Unit.
/// Each attribute has a base value and a flat bonus (e.g. from equipment or buffs).
/// FinalValue = BaseValue + FlatBonus
/// </summary>
public class UnitAttributes : MonoBehaviour
{
    // ─── Serialized Data ────────────────────────────────────────────────

    [Serializable]
    public struct BaseAttributesData
    {
        [Header("Power Pools")]
        public int health;
        public int mana;
        public int stamina;

        [Header("Primary Attributes")]
        public int strength;
        public int agility;
        public int intellect;
        public int spirit;

        [Header("Combat")]
        public int armor;
        public int attackPower;
        public int spellPower;
    }

    [Serializable]
    public struct Regeneration
    {
        public float healthPerSecond;
        public float manaPerSecond;
        public float staminaPerSecond;
    }

    // ─── Events ─────────────────────────────────────────────────────────

    public event Action<int, int> OnHealthChanged;
    public event Action<int, int> OnManaChanged;
    public event Action<int, int> OnStaminaChanged;
    public event Action<PowerType, int, int> OnPowerChanged;
    public event Action<AttributeType, float> OnAttributeChanged;
    public event Action OnDeath;

    // ─── Accessors ───────────────────────────────────────────────────────

    public int CurrentHealth  => currentPower[(int)PowerType.Health];
    public int CurrentMana    => currentPower[(int)PowerType.Mana];
    public int CurrentStamina => currentPower[(int)PowerType.Stamina];
    public int MaxHealth      => maxPower[(int)PowerType.Health];
    public int MaxMana        => maxPower[(int)PowerType.Mana];
    public int MaxStamina     => maxPower[(int)PowerType.Stamina];

    // ─── Unity Lifecycle ────────────────────────────────────────────────

    private void Start()
    {
        InitializeAttributes();
    }

    private void Update()
    {
        ProcessRegeneration(Time.deltaTime);
    }

    // ─── Initialization ─────────────────────────────────────────────────

    public void InitializeAttributes()
    {
        baseValues[(int)AttributeType.Strength]   = baseAttributes.strength;
        baseValues[(int)AttributeType.Agility]     = baseAttributes.agility;
        baseValues[(int)AttributeType.Stamina]     = baseAttributes.stamina;
        baseValues[(int)AttributeType.Intellect]   = baseAttributes.intellect;
        baseValues[(int)AttributeType.Spirit]      = baseAttributes.spirit;
        baseValues[(int)AttributeType.Armor]       = baseAttributes.armor;
        baseValues[(int)AttributeType.AttackPower] = baseAttributes.attackPower;
        baseValues[(int)AttributeType.SpellPower]  = baseAttributes.spellPower;

        Array.Clear(bonusValues, 0, bonusValues.Length);

        maxPower[(int)PowerType.Health]  = baseAttributes.health;
        maxPower[(int)PowerType.Mana]    = baseAttributes.mana;
        maxPower[(int)PowerType.Stamina] = baseAttributes.stamina;

        for (int i = 0; i < (int)PowerType.Count; i++)
            currentPower[i] = maxPower[i];

        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        OnManaChanged?.Invoke(CurrentMana, MaxMana);
        OnStaminaChanged?.Invoke(CurrentStamina, MaxStamina);
    }

    /// <summary>
    /// Sets base attributes from external data (e.g. CreatureTemplateSO).
    /// </summary>
    public void SetBaseAttributes(BaseAttributesData data)
    {
        baseAttributes = data;
        InitializeAttributes();
    }

    // ─── Attribute Accessors ────────────────────────────────────────────

    /// <summary>
    /// Returns the final value: base + flat bonus.
    /// </summary>
    public float GetAttribute(AttributeType attribute)
        => baseValues[(int)attribute] + bonusValues[(int)attribute];

    /// <summary>
    /// Returns the raw base value (before bonuses).
    /// </summary>
    public float GetBaseAttribute(AttributeType attribute)
        => baseValues[(int)attribute];

    /// <summary>
    /// Sets the base value of an attribute.
    /// </summary>
    public void SetBaseAttribute(AttributeType attribute, float value)
    {
        int idx = (int)attribute;
        baseValues[idx] = value;
        NotifyAttributeChanged(attribute);
    }

    /// <summary>
    /// Adds a flat bonus to an attribute (e.g. from equipment or a buff).
    /// </summary>
    public void AddBonus(AttributeType attribute, float amount)
    {
        bonusValues[(int)attribute] += amount;
        NotifyAttributeChanged(attribute);
    }

    /// <summary>
    /// Removes a flat bonus from an attribute.
    /// </summary>
    public void RemoveBonus(AttributeType attribute, float amount)
    {
        bonusValues[(int)attribute] -= amount;
        NotifyAttributeChanged(attribute);
    }

    /// <summary>
    /// Resets all flat bonuses on an attribute to zero.
    /// </summary>
    public void ClearBonuses(AttributeType attribute)
    {
        bonusValues[(int)attribute] = 0f;
        NotifyAttributeChanged(attribute);
    }

    private void NotifyAttributeChanged(AttributeType attribute)
        => OnAttributeChanged?.Invoke(attribute, GetAttribute(attribute));

    // ─── Power System (Health, Mana, Stamina) ───────────────────────────

    public int GetPower(PowerType power)    => currentPower[(int)power];
    public int GetMaxPower(PowerType power) => maxPower[(int)power];

    public void SetMaxPower(PowerType power, int value)
    {
        int idx = (int)power;
        maxPower[idx] = value;
        if (currentPower[idx] > maxPower[idx])
        {
            currentPower[idx] = maxPower[idx];
            NotifyPowerChanged(power);
        }
    }

    /// <summary>
    /// Modifies a power pool by the given amount (positive = gain, negative = cost/damage).
    /// Returns the actual delta applied.
    /// </summary>
    public int ModifyPower(PowerType power, int amount)
    {
        int idx      = (int)power;
        int previous = currentPower[idx];
        currentPower[idx] = Mathf.Clamp(currentPower[idx] + amount, 0, maxPower[idx]);

        int delta = currentPower[idx] - previous;
        if (delta != 0)
        {
            NotifyPowerChanged(power);

            if (power == PowerType.Health && currentPower[idx] <= 0 && previous > 0)
                OnDeath?.Invoke();
        }

        return delta;
    }

    private void NotifyPowerChanged(PowerType power)
    {
        int idx = (int)power;
        OnPowerChanged?.Invoke(power, currentPower[idx], maxPower[idx]);

        switch (power)
        {
            case PowerType.Health:
                OnHealthChanged?.Invoke(currentPower[idx], maxPower[idx]);
                break;
            case PowerType.Mana:
                OnManaChanged?.Invoke(currentPower[idx], maxPower[idx]);
                break;
            case PowerType.Stamina:
                OnStaminaChanged?.Invoke(currentPower[idx], maxPower[idx]);
                break;
        }
    }

    // ─── Convenience API ────────────────────────────────────────────────

    public void ModifyHealth(int amount)  => ModifyPower(PowerType.Health, amount);
    public void ModifyMana(int amount)    => ModifyPower(PowerType.Mana, amount);
    public void ModifyStamina(int amount) => ModifyPower(PowerType.Stamina, amount);

    public bool HasEnoughMana(int cost)    => CurrentMana >= cost;
    public bool HasEnoughStamina(int cost) => CurrentStamina >= cost;
    public bool IsAlive()                  => CurrentHealth > 0;

    public void SetRegeneration(Regeneration newRegeneration)
        => regeneration = newRegeneration;

    // ─── Regeneration ───────────────────────────────────────────────────

    private void ProcessRegeneration(float deltaTime)
    {
        if (CurrentHealth <= 0) return;

        TryRegenerate(PowerType.Health,  regeneration.healthPerSecond,  ref healthRegenAccumulator,  deltaTime);
        TryRegenerate(PowerType.Mana,    regeneration.manaPerSecond,    ref manaRegenAccumulator,    deltaTime);
        TryRegenerate(PowerType.Stamina, regeneration.staminaPerSecond, ref staminaRegenAccumulator, deltaTime);
    }

    private void TryRegenerate(PowerType power, float ratePerSecond, ref float accumulator, float deltaTime)
    {
        int idx = (int)power;
        if (currentPower[idx] >= maxPower[idx]) return;

        accumulator += ratePerSecond * deltaTime;
        if (accumulator >= 1f)
        {
            int amount = Mathf.FloorToInt(accumulator);
            accumulator -= amount;
            ModifyPower(power, amount);
        }
    }

    [Header("Base Attributes")]
    [SerializeField] private BaseAttributesData baseAttributes;

    [Header("Regeneration")]
    [SerializeField] private Regeneration regeneration;

    private readonly int[] currentPower = new int[(int)PowerType.Count];
    private readonly int[] maxPower = new int[(int)PowerType.Count];
    private readonly float[] baseValues = new float[(int)AttributeType.Count];
    private readonly float[] bonusValues = new float[(int)AttributeType.Count];
    private float healthRegenAccumulator;
    private float manaRegenAccumulator;
    private float staminaRegenAccumulator;
}

