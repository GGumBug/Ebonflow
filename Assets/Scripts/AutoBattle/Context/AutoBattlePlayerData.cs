using System;

namespace AutoBattle
{
    [Serializable]
    public class AutoBattlePlayerData
    {
        public int level;
        public int soulCoin;

        public AutoBattlePlayerData(int level, int soulCoin)
        {
            this.level = level;
            this.soulCoin = soulCoin;
        }
    }
}
