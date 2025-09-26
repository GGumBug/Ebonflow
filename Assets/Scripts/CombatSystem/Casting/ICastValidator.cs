using SkillSystem;
using System.Collections.Generic;

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
}