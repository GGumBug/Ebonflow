public class AutoBattleStateController
{
    private AutoBattleGameState _gameState = AutoBattleGameState.Setup;

    public AutoBattleGameState GameState
    {
        get { return _gameState; }
        set
        {
            _gameState = value;

            switch (_gameState)
            {
                case AutoBattleGameState.Setup:
                    break;
                case AutoBattleGameState.Starting:
                    break;
                case AutoBattleGameState.InProgress:
                    break;
                case AutoBattleGameState.Paused:
                    break;
                case AutoBattleGameState.Victory:
                    break;
                case AutoBattleGameState.Defeat:
                    break;
            }
        }

    }
}
