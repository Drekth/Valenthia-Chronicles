using Sisus.Init;


/// <summary>
/// Returns the spell's base damage without any scaling.
/// This matches the V1 behavior and serves as the default calculator.
/// </summary>
[Service(typeof(IDamageCalculator))]
public sealed class FlatDamageCalculator : IDamageCalculator
{
    public int Calculate(SpellDamageContext context)
    {
        return context.BaseDamage;
    }
}
