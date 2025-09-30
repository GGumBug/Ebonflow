using ProjectileSystem;
using SkillSystem;
using UnityEngine;

namespace CombatSystem
{
    public class ProjectileSkillExecutor : SkillExecutor
    {
        private ProjectileManager _projectileManager;

        public ProjectileSkillExecutor()
        {
            _projectileManager = ProjectileManager.Instance;
        }

        public override void Execute(IAttacker attacker, SkillDefinition skillDefinition, ValidationResult validationResult, DamageCalculator damageCalculator)
        {
            if (!validationResult.Accepted)
            {
                Debug.LogError("Targeted skill executed without a target.");
                return;
            }

            foreach (var target in validationResult.Targets)
            {
                int damage = damageCalculator.CalculateDamage(attacker.Stat, target.Stat);
                _projectileManager.LaunchProjectile(attacker, target, skillDefinition, validationResult, damageCalculator, ApplyDamage);
            }
        }
    }
}