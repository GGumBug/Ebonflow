using SkillSystem;
using UnityEngine;

namespace CombatSystem
{
    public class TargetedSkillExecutor : ISkillExecutor
    {
        public void Execute(IAttacker attacker, SkillDefinition skillDefinition, ValidationResult validationResult, DamageCalculator damageCalculator)
        {
            if (!validationResult.Accepted)
            {
                Debug.LogError("Targeted skill executed without a target.");
                return;
            }

            foreach (var target in validationResult.Targets)
            {
                int damage = damageCalculator.CalculateDamage(attacker.Stat, target.Stat);
                int appliedDamage = 0;
                target.Health.ApplyDamageAndGetApplied(damage, out appliedDamage);
            }
        }
    }
}