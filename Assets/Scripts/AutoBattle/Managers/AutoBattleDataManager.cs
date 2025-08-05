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

        public void Reset()
        {
            AutoBattleSceneDataContext.Reset();
        }

        private void OnDestroy()
        {
            AutoBattlePlayerDataContext.Save();
            AutoBattleSceneDataContext.Save();
        }
    }
}