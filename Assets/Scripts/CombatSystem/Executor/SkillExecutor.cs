using SkillSystem;

namespace CombatSystem
{
    public abstract class SkillExecutor : ISkillExecutor
    {
        public abstract void Execute(IAttacker attacker, SkillDefinition skillDefinition, ValidationResult validationResult, DamageCalculator damageCalculator);

        protected void ApplyDamage(IAttacker attacker, IVictim victim, DamageCalculator damageCalculator)
        {
            int damage = damageCalculator.CalculateDamage(attacker.Stat, victim.Stat);
            int appliedDamage = 0;
            victim.Health.ApplyDamageAndGetApplied(damage, out appliedDamage);
        }
    }
}