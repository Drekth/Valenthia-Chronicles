public readonly struct SpellCastStartedEvent
{
    public Unit Caster { get; }
    public Unit Target { get; }
    public SpellDataSO SpellData { get; }
    public float CastTime { get; }

    public SpellCastStartedEvent(Unit caster, Unit target, SpellDataSO spellData, float castTime)
    {
        Caster = caster;
        Target = target;
        SpellData = spellData;
        CastTime = castTime;
    }
}
