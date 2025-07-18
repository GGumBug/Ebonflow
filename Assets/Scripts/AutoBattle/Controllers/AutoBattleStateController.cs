using System;

namespace AutoBattle
{
    public class AutoBattleStateController
    {
        private AutoBattleGameState _gameState = AutoBattleGameState.Setup;

        public event Action OnSetup;
        public event Action OnStarting;
        public event Action OnPreparationPhase;
        public event Action OnBattlePhase;
        public event Action OnResolutionPhase;
        public event Action OnPause;
        public event Action OnVictory;
        public event Action OnDefeat;

        public AutoBattleGameState GameState
        {
            get => _gameState;
            set
            {
                if (_gameState == value) return;
                _gameState = value;

                switch (_gameState)
                {
                    case AutoBattleGameState.Setup:
                        OnSetup?.Invoke();
                        break;
                    case AutoBattleGameState.Starting:
                        OnStarting?.Invoke();
                        break;
                    case AutoBattleGameState.PreparationPhase:
                        OnPreparationPhase?.Invoke();
                        break;
                    case AutoBattleGameState.BattlePhase:
                        OnBattlePhase?.Invoke();
                        break;
                    case AutoBattleGameState.ResolutionPhase:
                        OnResolutionPhase?.Invoke();
                        break;
                    case AutoBattleGameState.Pause:
                        OnPause?.Invoke();
                        break;
                    case AutoBattleGameState.Victory:
                        OnVictory?.Invoke();
                        break;
                    case AutoBattleGameState.Defeat:
                        OnDefeat?.Invoke();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

}