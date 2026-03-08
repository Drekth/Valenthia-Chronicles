using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
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

    private void Awake()
    {
        player = GetComponentInParent<Player>();
    }

    private void Update()
    {
        UpdateHealthBar();
        UpdateManaBar();
    }

    [Header("Health Bar")]
    [SerializeField] private Image healthBar;

    [Header("Mana Bar")]
    [SerializeField] private Image manaBar;

    private Player player;
}
