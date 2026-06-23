using System.Collections;
using UnityEngine;

// Out-of-combat regeneration for health and mana. Add to any unit that should regen when idle
// (typically the player). The regen coroutine runs continuously; it only applies values when the
// unit is alive and out of combat, so combat does not inhibit the tick — it just suppresses the
// effect.
//
// Rates are per second; TickInterval controls granularity (1 s = one restore call per second).
// Lower TickInterval = smoother regen on screen, but more event traffic — 1 s is a good default.
[RequireComponent(typeof(Unit))]
public class RegenComponent : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void Awake()
    {
        OwnerUnit = GetComponent<Unit>();
    }

    private void OnEnable()
    {
        RegenCoroutine = StartCoroutine(RegenTick());
    }

    private void OnDisable()
    {
        if (RegenCoroutine != null)
        {
            StopCoroutine(RegenCoroutine);
            RegenCoroutine = null;
        }
    }

    private IEnumerator RegenTick()
    {
        while (true)
        {
            yield return new WaitForSeconds(TickInterval);

            if (OwnerUnit.IsDead || OwnerUnit.InCombat)
            {
                continue;
            }

            float Restored = HealthRegenPerSecond * TickInterval;
            if (Restored > 0.0f)
            {
                OwnerUnit.RestoreHealth(Restored);
            }

            float RestoredMana = ManaRegenPerSecond * TickInterval;
            if (RestoredMana > 0.0f)
            {
                OwnerUnit.RestoreMana(RestoredMana);
            }
        }
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    [Header("Regeneration")]
    [SerializeField] private float HealthRegenPerSecond = 5.0f;
    [SerializeField] private float ManaRegenPerSecond   = 10.0f;
    [SerializeField] private float TickInterval         = 1.0f;

    private Unit OwnerUnit;
    private Coroutine RegenCoroutine;
}
