using SkillSystem;
using System.Collections.Generic;
using AutoBattle;

namespace CombatSystem
{
    public interface ICastValidator
    {
        ValidationResult Validate(SkillDefinition skill, IAttacker attacker, IRangeDetector detector);
    }

    public sealed class RequireEnemyInRangeValidator : ICastValidator
    {
        public ValidationResult Validate(SkillDefinition skill, IAttacker attacker, IRangeDetector detector)
        {
            if (detector == null || !detector.HasEnemies)
                return ValidationResult.Fail("NoEnemyInRange");

            var closestEnemy = detector.GetClosestEnemy();
            if (closestEnemy == null)
                return ValidationResult.Fail("NoValidTarget");

            var targets = new List<IVictim> { closestEnemy };
            return ValidationResult.Ok(targets: targets);
        }
    }

    public sealed class StrongestEnemyValidator : ICastValidator
    {
        private AutoBattleUnitManager unitManager;

        public ValidationResult Validate(SkillDefinition skill, IAttacker attacker, IRangeDetector detector)
        {
            unitManager ??= AutoBattleUnitManager.Instance;

            var opposingTeam = unitManager.Roster.GetOpposingTeam((TeamType)attacker.TeamId);

            Unit strongestTarget = null;
            float highestDps = -1f;

            foreach (var opposingUnit in opposingTeam)
            {
                if (opposingUnit == null || opposingUnit.IsDead)
                    continue;

                float unitDps = opposingUnit.Stat.GetDPS();

                if (unitDps > highestDps)
                {
                    highestDps = unitDps;
                    strongestTarget = opposingUnit;
                }
            }

            if (strongestTarget != null)
            {
                var targets = new List<IVictim> { strongestTarget };
                return ValidationResult.Ok(targets: targets);
            }
            else
            {
                return ValidationResult.Fail("NoStrongestTargetAvailable");
            }
        }
    }
}