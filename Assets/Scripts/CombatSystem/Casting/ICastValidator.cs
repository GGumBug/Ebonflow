using SkillSystem;

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

            // 범위 내에만 있으면 통과 → 타겟팅은 Detector가 후처리 가능
            var target = detector.GetClosestEnemy();
            return target != null
                ? ValidationResult.Ok(target: target)
                : ValidationResult.Fail("NoValidTarget");
        }
    }
}