using System.Collections.Generic;

namespace AutoBattle
{
    public interface IBattleRoster
    {
        IReadOnlyCollection<Unit> Allies { get; }
        IReadOnlyCollection<Unit> Enemies { get; }

        void Register(Unit unit);
        void Unregister(Unit unit);
        bool Contains(Unit unit);
    }
}