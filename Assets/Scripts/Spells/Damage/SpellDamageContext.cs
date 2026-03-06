using Entities;


public readonly struct SpellDamageContext
{
    public SpellDataSO SpellData { get; }
    public Unit Caster { get; }
    public Unit Target { get; }

    public int BaseDamage => SpellData.Damage;
    public DamageType DamageType => SpellData.DamageType;

    public SpellDamageContext(SpellDataSO spellData, Unit caster, Unit target)
    {
        SpellData = spellData;
        Caster = caster;
        Target = target;
    }
}
