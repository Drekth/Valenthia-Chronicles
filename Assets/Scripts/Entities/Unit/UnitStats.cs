using System;
using UnityEngine;
using VInspector;

namespace Entities
{
    public class UnitStats : MonoBehaviour
    {
        [Serializable]
        public struct Stats
        {
            public int health;
            public int mana;
            public int stamina;
        }

        [Serializable]
        public struct Regeneration
        {
            public float healthPerSecond;
            public float manaPerSecond;
            public float staminaPerSecond;
        }

        [Header("Maximum Stats")]
        [SerializeField] private Stats maxStats;

        [Header("Regeneration")]
        [SerializeField] private Regeneration regeneration;

        [Header("Current Stats")]
        [ReadOnly] public Stats currentStats;

        public event Action<int, int> OnHealthChanged;
        public event Action<int, int> OnManaChanged;
        public event Action<int, int> OnStaminaChanged;
        public event Action OnDeath;

        public int CurrentHealth => currentStats.health;
        public int CurrentMana => currentStats.mana;
        public int CurrentStamina => currentStats.stamina;
        public int MaxHealth => maxStats.health;
        public int MaxMana => maxStats.mana;
        public int MaxStamina => maxStats.stamina;

        private float healthRegenAccumulator;
        private float manaRegenAccumulator;
        private float staminaRegenAccumulator;

        private void Start()
        {
            InitializeStats();
        }

        private void Update()
        {
            ProcessRegeneration(Time.deltaTime);
        }

        public void InitializeStats()
        {
            currentStats = maxStats;
            OnHealthChanged?.Invoke(currentStats.health, maxStats.health);
            OnManaChanged?.Invoke(currentStats.mana, maxStats.mana);
            OnStaminaChanged?.Invoke(currentStats.stamina, maxStats.stamina);
        }

        private void ProcessRegeneration(float deltaTime)
        {
            if (currentStats.health <= 0)
                return;

            RegenerateHealth(deltaTime);
            RegenerateMana(deltaTime);
            RegenerateStamina(deltaTime);
        }

        private void RegenerateHealth(float deltaTime)
        {
            if (currentStats.health >= maxStats.health)
                return;

            healthRegenAccumulator += regeneration.healthPerSecond * deltaTime;

            if (healthRegenAccumulator >= 1f)
            {
                int regenAmount = Mathf.FloorToInt(healthRegenAccumulator);
                healthRegenAccumulator -= regenAmount;
                ModifyHealth(regenAmount);
            }
        }

        private void RegenerateMana(float deltaTime)
        {
            if (currentStats.mana >= maxStats.mana)
                return;

            manaRegenAccumulator += regeneration.manaPerSecond * deltaTime;

            if (manaRegenAccumulator >= 1f)
            {
                int regenAmount = Mathf.FloorToInt(manaRegenAccumulator);
                manaRegenAccumulator -= regenAmount;
                ModifyMana(regenAmount);
            }
        }

        private void RegenerateStamina(float deltaTime)
        {
            if (currentStats.stamina >= maxStats.stamina)
                return;

            staminaRegenAccumulator += regeneration.staminaPerSecond * deltaTime;

            if (staminaRegenAccumulator >= 1f)
            {
                int regenAmount = Mathf.FloorToInt(staminaRegenAccumulator);
                staminaRegenAccumulator -= regenAmount;
                ModifyStamina(regenAmount);
            }
        }

        public void ModifyHealth(int amount)
        {
            int previousHealth = currentStats.health;
            currentStats.health = Mathf.Clamp(currentStats.health + amount, 0, maxStats.health);

            if (previousHealth != currentStats.health)
            {
                OnHealthChanged?.Invoke(currentStats.health, maxStats.health);

                if (currentStats.health <= 0 && previousHealth > 0)
                {
                    OnDeath?.Invoke();
                }
            }
        }

        public void ModifyMana(int amount)
        {
            int previousMana = currentStats.mana;
            currentStats.mana = Mathf.Clamp(currentStats.mana + amount, 0, maxStats.mana);

            if (previousMana != currentStats.mana)
            {
                OnManaChanged?.Invoke(currentStats.mana, maxStats.mana);
            }
        }

        public void ModifyStamina(int amount)
        {
            int previousStamina = currentStats.stamina;
            currentStats.stamina = Mathf.Clamp(currentStats.stamina + amount, 0, maxStats.stamina);

            if (previousStamina != currentStats.stamina)
            {
                OnStaminaChanged?.Invoke(currentStats.stamina, maxStats.stamina);
            }
        }

        public bool HasEnoughMana(int cost)
        {
            return currentStats.mana >= cost;
        }

        public bool HasEnoughStamina(int cost)
        {
            return currentStats.stamina >= cost;
        }

        public bool IsAlive()
        {
            return currentStats.health > 0;
        }

        public void SetMaxStats(Stats newMaxStats)
        {
            maxStats = newMaxStats;
            currentStats.health = Mathf.Clamp(currentStats.health, 0, maxStats.health);
            currentStats.mana = Mathf.Clamp(currentStats.mana, 0, maxStats.mana);
            currentStats.stamina = Mathf.Clamp(currentStats.stamina, 0, maxStats.stamina);
        }

        public void SetRegeneration(Regeneration newRegeneration)
        {
            regeneration = newRegeneration;
        }
    }
}
