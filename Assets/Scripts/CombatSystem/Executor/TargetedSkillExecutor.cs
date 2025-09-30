using SkillSystem;
using UnityEngine;

namespace CombatSystem
{
    public class TargetedSkillExecutor : SkillExecutor
    {
        public override void Execute(IAttacker attacker, SkillDefinition skillDefinition, ValidationResult validationResult, DamageCalculator damageCalculator)
        {
            if (!validationResult.Accepted)
            {
                Debug.LogError("Targeted skill executed without a target.");
                return;
            }

            foreach (var target in validationResult.Targets)
                ApplyDamage(attacker, target, damageCalculator);
        }

    }
}