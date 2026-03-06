public readonly struct SpellCooldownEvent
{
    public SpellDataSO SpellData { get; }
    public float Duration { get; }
    public float Remaining { get; }

    public SpellCooldownEvent(SpellDataSO spellData, float duration, float remaining)
    {
        SpellData = spellData;
        Duration = duration;
        Remaining = remaining;
    }
}
