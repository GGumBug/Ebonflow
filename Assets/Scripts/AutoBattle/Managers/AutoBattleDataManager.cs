namespace AutoBattle
{
    public class AutoBattleDataManager : Singleton<AutoBattleDataManager>, IDonDestroy
    {
        private AutoBattleSceneContext _context;

        public void SetContext(AutoBattleSceneContext ctx) 
            => _context = ctx;

        public AutoBattleSceneContext GetContext() 
            => _context;

        public void Reset()
        {
            _context = default;
        }
    }    
}