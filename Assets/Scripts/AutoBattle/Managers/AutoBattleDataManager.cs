namespace AutoBattle
{
    public class AutoBattleDataManager : Singleton<AutoBattleDataManager>, IDonDestroy
    {
        public AutoBattleStageDataContext AutoBattleSceneDataContext { get; private set; }
        public AutoBattlePlayerDataContext AutoBattlePlayerDataContext { get; private set; }
        
        public void Setup()
        {
            AutoBattleSceneDataContext = new AutoBattleStageDataContext();
            AutoBattlePlayerDataContext = new AutoBattlePlayerDataContext();
        }

        public bool DeleteData()
        {
            bool sceneDataResult = AutoBattleSceneDataContext.Delete();
            bool battleDataResult = AutoBattlePlayerDataContext.Delete();

            if (sceneDataResult && battleDataResult)
                return true;

            return false;
        }
    }
}