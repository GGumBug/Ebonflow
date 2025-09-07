namespace CombatSystem
{
    public enum CastValidationPolicy
    {
        RequireEnemyInRange,
        RequireValidAimOnly,
        FreeCast
    }

    public enum TargetingType 
    { 
        Targeted, 
        Skillshot, 
        Area 
    }

    public enum DeliveryType 
    { 
        Instant, 
        Projectile, 
        Raycast 
    }
}