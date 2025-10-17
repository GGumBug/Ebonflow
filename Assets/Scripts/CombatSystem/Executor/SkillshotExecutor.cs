using ProjectileSystem;
using SkillSystem;
using System;
using UnityEngine;

namespace CombatSystem
{
    public class SkillshotExecutor : SkillExecutor
    {
        private ProjectileManager _projectileManager;

        public SkillshotExecutor()
        {
            _projectileManager = ProjectileManager.Instance;
        }

        public override void Execute(IAttacker attacker, SkillDefinition skillDefinition, ValidationResult validationResult, bool isManaGain, Action<IAttacker, Action> startAttackDelegate)
        {
            // 다수 공격 스킬에 대한 예외처리 필요
            Vector2 direction = (validationResult.Targets[0].Position - attacker.Position).normalized;
            attacker.Model.SetUnitDirection(direction);

            if (!validationResult.Accepted)
            {
                Debug.LogError("Targeted skill executed without a target.");
                return;
            }

            foreach (var target in validationResult.Targets)
            {
                startAttackDelegate.Invoke(attacker, () => _projectileManager.LaunchProjectile(attacker, target, skillDefinition, validationResult, combatManager.Calculator, isManaGain, ApplyDamage, target.Position));
            }
        }
    }
}