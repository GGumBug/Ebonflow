using System;
using System.Collections.Generic;

namespace AutoBattle
{
    [Serializable]
    public class AutoBattlePlayerData
    {
        public int level;
        public int soulCoin;
        public int winLossStreak;  // 연승 또는 연패 횟수 (양수: 연승, 음수: 연패)
        public int nextInstanceId;

        public List<PlayerUnitRecord> ownedUnits;
        public List<UnitPlacementRecord> placements;

        public AutoBattlePlayerData(int level, int soulCoin)
        {
            this.level = level;
            this.soulCoin = soulCoin;
            winLossStreak = 0;
            nextInstanceId = 1;
            ownedUnits = new();
            placements = new();
        }
    }
}
