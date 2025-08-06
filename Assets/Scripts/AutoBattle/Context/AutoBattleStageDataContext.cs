namespace AutoBattle
{
    public class AutoBattleStageDataContext : DataContext<AutoBattleStageData>
    {
        public AutoBattleStageDataContext()
            : base(
                fileName: "StageData",
                serializer: new AutoBattleStageDataSaveLoad(),
                defaultFactory: () => new AutoBattleStageData(shouldResumeBattle: false, stageNumber: 1, floor: 0, locationTypeId: 0, stageID: -1)
            )
        { }

        public AutoBattleStageData Stage => Data;

        public void SetShouldResumeBattle(bool shouldResumeBattle) { Data.shouldResumeBattle = shouldResumeBattle; }
        public void SetStageID(int stageID) { Data.stageID = stageID; } 
    }
}