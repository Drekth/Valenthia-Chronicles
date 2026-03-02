using System.Collections.Generic;
using Entities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SpellSystem
{
    public enum CastType
    {
        Instant,
        Casted
    }

    public enum TargetType
    {
        Enemy,
        Ally,
        Self
    }
    
    public class SpellCaster : MonoBehaviour
    {
        [Header("Spells")]
        [SerializeField] private List<SpellData> spells = new List<SpellData>();

        private Unit owner;
        private readonly Dictionary<SpellData, float> cooldowns = new Dictionary<SpellData, float>();

        private void Awake()
        {
            owner = GetComponent<Unit>();
        }

        private void Update()
        {
            UpdateCooldowns();
            HandleSpellInput();
        }

        private void HandleSpellInput()
        {
            if (Keyboard.current == null)
                return;

            if (Keyboard.current.digit1Key.wasPressedThisFrame)
                CastSpellAtIndex(0);
            else if (Keyboard.current.digit2Key.wasPressedThisFrame)
                CastSpellAtIndex(1);
            else if (Keyboard.current.digit3Key.wasPressedThisFrame)
                CastSpellAtIndex(2);
            else if (Keyboard.current.digit4Key.wasPressedThisFrame)
                CastSpellAtIndex(3);
            else if (Keyboard.current.digit5Key.wasPressedThisFrame)
                CastSpellAtIndex(4);
        }

        private void CastSpellAtIndex(int index)
        {
            if (index < 0 || index >= spells.Count)
                return;

            SpellData spell = spells[index];
            if (spell == null)
                return;

            CastSpell(spell);
        }

        private Unit GetTarget(SpellData spell)
        {
            switch (spell.TargetType)
            {
                case TargetType.Self:
                    return owner;
                    
                case TargetType.Enemy:
                case TargetType.Ally:
                    return owner.GetCurrentTarget();
                    
                default:
                    return null;
            }
        }

        private Transform GetVisualTransform(Unit target)
        {
            if (target is Player player)
            {
                Transform playerTransform = player.GetPlayerTransform();
                if (playerTransform != null)
                    return playerTransform;
            }

            return target.transform;
        }

        #region Spells
        public bool CanCast(SpellData spell)
        {
            if (spell == null)
                return false;

            if (!HasEnoughMana(spell))
                return false;

            if (IsOnCooldown(spell))
                return false;

            return true;
        }

        public bool CastSpell(SpellData spell)
        {
            if (!CanCast(spell))
            {
                Debug.LogWarning($"[SpellCaster] Cannot cast {spell.SpellName}");
                return false;
            }

            Unit target = GetTarget(spell);

            if (target == null)
            {
                Debug.LogWarning($"[SpellCaster] No valid target for {spell.SpellName}");
                return false;
            }

            ConsumeMana(spell);
            ApplyDamage(spell, target);
            SpawnVFX(spell, target);
            StartCooldown(spell);

            Debug.Log($"[SpellCaster] {owner.name} cast {spell.SpellName} on {target.name} for {spell.Damage} damage");

            return true;
        }

        private bool HasEnoughMana(SpellData spell)
        {
            return owner.CurrentMana >= spell.ManaCost;
        }

        private void ConsumeMana(SpellData spell)
        {
            if (spell.ManaCost > 0)
            {
                owner.Stats.ModifyMana(-spell.ManaCost);
            }
        }

        private void ApplyDamage(SpellData spell, Unit target)
        {
            if (spell.Damage > 0)
            {
                target.Stats.ModifyHealth(-spell.Damage);
            }
        }

        private void SpawnVFX(SpellData spell, Unit target)
        {
            if (spell.HasVFX)
            {
                Transform vfxTransform = GetVisualTransform(target);
                GameObject vfx = Instantiate(spell.ImpactVFXPrefab, vfxTransform.position, Quaternion.identity, vfxTransform);
                Destroy(vfx, spell.VFXDuration);
            }
        }

        private bool IsOnCooldown(SpellData spell)
        {
            return cooldowns.ContainsKey(spell) && cooldowns[spell] > 0f;
        }

        private void StartCooldown(SpellData spell)
        {
            if (spell.HasCooldown)
            {
                cooldowns[spell] = spell.Cooldown;
            }
        }

        private void UpdateCooldowns()
        {
            var keys = new List<SpellData>(cooldowns.Keys);
            foreach (var spell in keys)
            {
                if (cooldowns[spell] > 0f)
                {
                    cooldowns[spell] -= Time.deltaTime;
                }
            }
        }

        public float GetCooldownRemaining(SpellData spell)
        {
            return cooldowns.TryGetValue(spell, out var cooldown) ? Mathf.Max(0f, cooldown) : 0f;
        }
        #endregion

    }
}
