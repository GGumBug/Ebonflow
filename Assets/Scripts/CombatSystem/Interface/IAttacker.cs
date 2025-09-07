using UnityEngine;

namespace CombatSystem
{
    public interface IAttacker
    {
        public int TeamId { get; }
        public UnitClass Class { get; }
        public Vector3 Position { get; }
        public UnitStats Stat { get; }
        public ManaComponent Mana { get; }
    }
}
