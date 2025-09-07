namespace CombatSystem
{
    public interface IVictim
    {
        bool IsDead { get; }
        public UnitStats Stat { get; }
        public HealthComponent Health { get; }
    }
}