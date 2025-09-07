namespace CombatSystem
{
    public interface IAttacker
    {
        public UnitClass Class { get; }
        public UnitStats Stat { get; }
        public ManaComponent Mana { get; }
    }
}
