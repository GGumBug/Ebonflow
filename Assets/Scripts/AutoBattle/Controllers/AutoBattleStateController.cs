using System;

namespace AutoBattle
{
    public class AutoBattleStateController
    {
        private AutoBattleGameState _gameState = AutoBattleGameState.Setup;

        // 우선순위 이벤트 컬렉션
        public PriorityEvent SetupEntered { get; } = new();
        public PriorityEvent StartingEntered { get; } = new();
        public PriorityEvent PreparationEntered { get; } = new();
        public PriorityEvent BattleEntered { get; } = new();
        public PriorityEvent ResolutionEntered { get; } = new();
        public PriorityEvent PauseEntered { get; } = new();
        public PriorityEvent VictoryEntered { get; } = new();
        public PriorityEvent DefeatEntered { get; } = new();

        public AutoBattleGameState GameState
        {
            get => _gameState;
            set
            {
                if (_gameState == value) return;
                _gameState = value;

                switch (_gameState)
                {
                    case AutoBattleGameState.Setup: SetupEntered.Invoke(); break;
                    case AutoBattleGameState.Starting: StartingEntered.Invoke(); break;
                    case AutoBattleGameState.PreparationPhase: PreparationEntered.Invoke(); break;
                    case AutoBattleGameState.BattlePhase: BattleEntered.Invoke(); break;
                    case AutoBattleGameState.Victory: VictoryEntered.Invoke(); break;
                    case AutoBattleGameState.Defeat: DefeatEntered.Invoke(); break;
                    case AutoBattleGameState.ResolutionPhase: ResolutionEntered.Invoke(); break;
                    case AutoBattleGameState.Pause: PauseEntered.Invoke(); break;
                    default: throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}