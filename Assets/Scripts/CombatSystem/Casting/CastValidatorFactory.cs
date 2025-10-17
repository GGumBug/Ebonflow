using CombatSystem;

public sealed class CastValidatorFactory
{
    private readonly ICastValidator _enemyInRange = new RequireEnemyInRangeValidator();
    private readonly ICastValidator _strongestEnemy = new StrongestEnemyValidator();

    public ICastValidator Get(TargetingType policy) => policy switch
    {
        TargetingType.Targeted => _enemyInRange,
        TargetingType.Area => _strongestEnemy,
        _ => _enemyInRange
    };
}