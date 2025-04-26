using System;

public class HealthComponent
{
    private UnitStats _stats;
    public event Action OnDied;

    public HealthComponent(UnitStats stats)
    {
        _stats = stats;
    }

    public bool ApplyDamage(int dmg)
    {
        _stats.TakeDamage(dmg);

        if (_stats.CurrentHP <= 0)
        {
            OnDied?.Invoke();
            return true;
        }

        return false;
    }
}
