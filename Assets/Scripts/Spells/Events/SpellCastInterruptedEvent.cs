using Entities;


public readonly struct SpellCastInterruptedEvent
{
    public Unit Caster { get; }
    public SpellDataSO SpellData { get; }

    public SpellCastInterruptedEvent(Unit caster, SpellDataSO spellData)
    {
        Caster = caster;
        SpellData = spellData;
    }
}
