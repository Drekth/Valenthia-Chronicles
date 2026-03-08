/// <summary>
/// Applies attribute-based scaling to spell damage.
/// Physical scales with Strength, Magical with Intellect (future attributes).
/// For now, uses flat damage as baseline until combat attributes are wired up.
/// </summary>
public sealed class ScaledDamageCalculator : IDamageCalculator
{
    public ScaledDamageCalculator(float scalingFactor = 1.0f)
    {
        this.scalingFactor = scalingFactor;
    }

    public int Calculate(SpellDamageContext context)
    {
        // TODO: When UnitAttributes gains combat wiring, scale based on DamageType:
        //   Physical → BaseDamage + (Strength * scalingFactor)
        //   Magical  → BaseDamage + (Intellect * scalingFactor)
        //   Hybrid   → BaseDamage + ((Strength + Intellect) / 2 * scalingFactor)
        return (int)(context.BaseDamage * scalingFactor);
    }

    private readonly float scalingFactor;
}
