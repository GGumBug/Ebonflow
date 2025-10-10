using UnityEngine;

namespace CombatSystem
{
    public interface IAttacker
    {
        public int TeamId { get; }
        public UnitClass Class { get; }
        public Vector3 Position { get; }
        public UnitModel Model { get; }
        public UnitStats Stat { get; }
        public ManaComponent Mana { get; }
        public int AttackSkillID { get; }
    }
}
