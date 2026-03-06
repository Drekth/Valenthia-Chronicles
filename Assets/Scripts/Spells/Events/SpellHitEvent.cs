using Entities;


public readonly struct SpellHitEvent
{
    public Unit Caster { get; }
    public Unit Target { get; }
    public SpellDataSO SpellData { get; }
    public int DamageDealt { get; }

    public SpellHitEvent(Unit caster, Unit target, SpellDataSO spellData, int damageDealt)
    {
        Caster = caster;
        Target = target;
        SpellData = spellData;
        DamageDealt = damageDealt;
    }
}
