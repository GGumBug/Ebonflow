using AutoBattle;

namespace AutoBattle.Input
{
    public class PlacementInputGate : IPlacementInputGate
    {
        public bool IsEnabled { get; private set; }

        public PlacementInputGate(AutoBattleStateController state)
        {
            state.PreparationEntered.Add(() => IsEnabled = true, 1);
            state.BattleEntered.Add(() => IsEnabled = true, 1);
            state.SetupEntered.Add(() => IsEnabled = false, 1);
            state.StartingEntered.Add(() => IsEnabled = false, 1);
            state.ResolutionEntered.Add(() => IsEnabled = false, 1);
            state.PauseEntered.Add(() => IsEnabled = false, 1);
            state.VictoryEntered.Add(() => IsEnabled = false, 1);
            state.DefeatEntered.Add(() => IsEnabled = false, 1);
        }
    }
}