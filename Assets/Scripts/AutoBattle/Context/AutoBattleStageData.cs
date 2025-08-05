using System;
using UnityEngine;

namespace AutoBattle
{
    [Serializable]
    public class AutoBattleStageData
    {
        public int StageNumber { get; }
        public int Floor { get; }
        public int LocationTypeId { get; }

        public AutoBattleStageData(int stageNumber, int floor, int locationTypeId)
        {
            StageNumber = stageNumber;
            Floor = floor;
            LocationTypeId = locationTypeId;
        }
    }
}