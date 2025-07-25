namespace AutoBattle
{
    public class AutoBattleDataManager : Singleton<AutoBattleDataManager>, IDonDestroy
    {
        private AutoBattleSceneContext _context;

        public AutoBattlePlayerDataContext AutoBattlePlayerDataContext { get; private set; }

        public void SetContext(AutoBattleSceneContext ctx) 
            => _context = ctx;

        public AutoBattleSceneContext GetContext() 
            => _context;

        public void Setup()
        {
            AutoBattlePlayerDataContext = new AutoBattlePlayerDataContext();
        }

        public void Reset()
        {
            _context = default;
        }
    }    
}