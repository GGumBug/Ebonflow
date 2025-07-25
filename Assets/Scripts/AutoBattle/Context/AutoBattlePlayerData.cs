using System;

namespace AutoBattle
{
    [Serializable]
    public class AutoBattlePlayerData
    {
        public int soulCoin;

        public AutoBattlePlayerData(int soulCoin)
        {
            this.soulCoin = soulCoin;
        }
    }
}
