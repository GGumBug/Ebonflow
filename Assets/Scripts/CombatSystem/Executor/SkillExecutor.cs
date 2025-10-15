using Cysharp.Threading.Tasks;
using SkillSystem;
using System;

namespace CombatSystem
{
    public abstract class SkillExecutor : ISkillExecutor
    {
        protected CombatManager combatManager;

        public SkillExecutor()
        {
            combatManager = CombatManager.Instance;
        }

        public abstract void Execute(IAttacker attacker, SkillDefinition skillDefinition, ValidationResult validationResult, bool isManaGain, Action<IAttacker, Action> startAttackDelegate);

        protected void ApplyDamage(IAttacker attacker, IVictim victim, DamageCalculator damageCalculator, bool isManaGain)
        {
            int appliedDamage = -1;
            int damage = damageCalculator.CalculateDamage(attacker.Stat, victim.Stat);
            victim.Health.ApplyDamageAndGetApplied(damage, out appliedDamage);

            if(isManaGain)
            {
                combatManager.ManaGainService.OnDealDamage(attacker, appliedDamage);
            }
        }
    }
}