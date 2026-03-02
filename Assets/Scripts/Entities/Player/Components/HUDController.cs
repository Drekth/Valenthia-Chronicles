using UnityEngine;
using UnityEngine.UI;

namespace Entities
{
    public class HUDController : MonoBehaviour
    {
        [Header("Health Bar")]
        [SerializeField] private Image healthBar;

        [Header("Mana Bar")]
        [SerializeField] private Image manaBar;

        private Player player;

        private void Awake()
        {
            player = GetComponentInParent<Player>();
        }

        private void Update()
        {
            UpdateHealthBar();
            UpdateManaBar();
        }

        private void UpdateHealthBar()
        {
            if (healthBar != null && player != null && player.MaxHealth > 0)
            {
                healthBar.fillAmount = (float)player.CurrentHealth / player.MaxHealth;
            }
        }

        private void UpdateManaBar()
        {
            if (manaBar != null && player != null && player.MaxMana > 0)
            {
                manaBar.fillAmount = (float)player.CurrentMana / player.MaxMana;
            }
        }
    }
}