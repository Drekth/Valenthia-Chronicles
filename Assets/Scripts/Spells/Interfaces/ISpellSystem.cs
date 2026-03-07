public interface ISpells
{
    bool CanCast(Unit caster, SpellDataSO spell);
    bool CastSpell(Unit caster, SpellDataSO spell, Unit target);
    float GetCooldownRemaining(Unit caster, SpellDataSO spell);
    bool IsOnCooldown(Unit caster, SpellDataSO spell);
    bool IsInRange(Unit caster, SpellDataSO spell, Unit target);
}
