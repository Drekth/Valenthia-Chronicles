using Entities;


public readonly struct SpellCastEvent
{
    public Unit Caster { get; }
    public Unit Target { get; }
    public SpellDataSO SpellData { get; }
    public float Timestamp { get; }

    public SpellCastEvent(Unit caster, Unit target, SpellDataSO spellData, float timestamp)
    {
        Caster = caster;
        Target = target;
        SpellData = spellData;
        Timestamp = timestamp;
    }
}
