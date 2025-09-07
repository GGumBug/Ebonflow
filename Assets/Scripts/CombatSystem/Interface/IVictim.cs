using System;
using UnityEngine;

namespace CombatSystem
{
    public interface IVictim
    {
        public int TeamId { get; }
        bool IsDead { get; }
        bool IsBattleActive { get; }
        public Vector3 Position { get; }
        public UnitStats Stat { get; }
        public HealthComponent Health { get; }

        event Action<IVictim> OnDied;
    }
}