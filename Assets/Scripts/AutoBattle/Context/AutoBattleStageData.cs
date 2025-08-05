using System;

namespace AutoBattle
{
    [Serializable]
    public class AutoBattleStageData
    {
        public bool shouldResumeBattle;
        public int  stageNumber;
        public int  floor;
        public int  locationTypeId;
        public int  stageID;

        public AutoBattleStageData(bool shouldResumeBattle, int stageNumber, int floor, int locationTypeId, int stageID)
        {
            this.shouldResumeBattle = shouldResumeBattle;
            this.stageNumber = stageNumber;
            this.floor = floor;
            this.locationTypeId = locationTypeId;
            this.stageID = stageID;
        }
    }
}