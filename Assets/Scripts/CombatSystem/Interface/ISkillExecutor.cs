using SkillSystem;

namespace CombatSystem
{
    public interface ISkillExecutor
    {
        void Execute(IAttacker attacker, SkillDefinition skillDefinition, ValidationResult validationResult, DamageCalculator damageCalculator);
    }
}