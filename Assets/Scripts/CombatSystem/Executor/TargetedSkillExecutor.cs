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

            int damage = damageCalculator.CalculateDamage(attacker.Stat, validationResult.Target.Stat);
            int appliedDamage = 0;
            validationResult.Target.Health.ApplyDamageAndGetApplied(damage, out appliedDamage);
        }
    }
}