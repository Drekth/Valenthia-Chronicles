using UnityEngine;

public class Unit : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    public float CurrentHealth => Health;
    public float MaximumHealth => MaxHealth;
    public bool IsSelectable => HasFlag(UnitFlags.IsSelectable);

    public bool HasFlag(UnitFlags Flag)
    {
        return (Flags & Flag) != 0;
    }

    public void TakeDamage(float Amount)
    {
        Health = Mathf.Max(0.0f, Health - Amount);
    }

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void Awake()
    {
        Health = MaxHealth;
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [Header("Health")]
    [SerializeField] private float MaxHealth = 100.0f;

    [Header("Flags")]
    [SerializeField] private UnitFlags Flags = UnitFlags.None;

    private float Health;
}