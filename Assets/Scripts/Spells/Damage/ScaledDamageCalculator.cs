/// <summary>
/// Applies stat-based scaling to spell damage.
/// Physical scales with Strength, Magical with Intelligence (future stats).
/// For now, uses flat damage as baseline until combat stats are added to UnitStats.
/// </summary>
public sealed class ScaledDamageCalculator : IDamageCalculator
{
    private readonly float scalingFactor;

    public ScaledDamageCalculator(float scalingFactor = 1.0f)
    {
        this.scalingFactor = scalingFactor;
    }

    public int Calculate(SpellDamageContext context)
    {
        // TODO: When UnitStats gains combat stats (Intelligence, Strength),
        // scale based on DamageType:
        //   Physical → BaseDamage + (Strength * scalingFactor)
        //   Magical  → BaseDamage + (Intelligence * scalingFactor)
        //   Hybrid   → BaseDamage + ((Strength + Intelligence) / 2 * scalingFactor)
        return (int)(context.BaseDamage * scalingFactor);
    }
}
