using UnityEngine;

// Bridges animation impact frames to the SpellCaster. Lives on the GameObject that carries the
// Animator (the body model), because AnimationEvents only invoke methods on components of that
// same object. The attack clip places an AnimationEvent on its swing frame calling OnAttackImpact.
public class SpellAnimationRelay : MonoBehaviour
{
    ////////////////////////////////////////////////////////////
    /// Public                                               ///
    ////////////////////////////////////////////////////////////

    // AnimationEvent target — keep this name in sync with the event set on the attack clip.
    public void OnAttackImpact()
    {
        if (Caster != null)
        {
            Caster.NotifyImpact();
        }
    }

    ////////////////////////////////////////////////////////////
    /// Private                                              ///
    ////////////////////////////////////////////////////////////

    private void Awake()
    {
        Caster = GetComponentInParent<SpellCaster>();
    }

    ////////////////////////////////////////////////////////////
    /// Fields                                               ///
    ////////////////////////////////////////////////////////////

    private SpellCaster Caster;
}
