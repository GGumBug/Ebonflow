using CombatSystem;

public sealed class CastValidatorFactory
{
    private readonly ICastValidator _enemyInRange = new RequireEnemyInRangeValidator();

    public ICastValidator Get(TargetingType policy) => policy switch
    {
        TargetingType.Targeted => _enemyInRange,
        _ => _enemyInRange
    };
}