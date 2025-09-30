using SkillSystem;
using UnityEngine;
using ProjectileSystem;

namespace CombatSystem
{
    public class TargetedSkillExecutor : SkillExecutor
    {
        private ProjectileManager _projectileManager;

        public TargetedSkillExecutor()
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

            if (skillDefinition.Delivery == DeliveryType.Instant)
            {
                foreach (var target in validationResult.Targets)
                    ApplyDamage(attacker, target, damageCalculator);
            }
            else if (skillDefinition.Delivery == DeliveryType.Projectile)
            {
                foreach (var target in validationResult.Targets)
                {
                    _projectileManager.LaunchProjectile(attacker, target, skillDefinition, validationResult, damageCalculator, ApplyDamage);
                }
            }
        }

    }
}