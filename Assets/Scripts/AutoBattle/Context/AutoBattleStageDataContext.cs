namespace AutoBattle
{
    public class AutoBattleStageDataContext : DataContext<AutoBattleStageData>
    {
        public AutoBattleStageDataContext()
            : base(
                fileName: "StageData",
                serializer: new AutoBattleStageDataSaveLoad(),
                defaultFactory: () => new AutoBattleStageData(stageNumber: 0, floor: 0, locationTypeId: 0)
            )
        { }

        public AutoBattleStageData Scene => Data;
    }
}