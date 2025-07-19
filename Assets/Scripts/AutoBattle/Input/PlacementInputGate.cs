using AutoBattle;

public class PlacementInputGate : IPlacementInputGate
{
    public bool IsEnabled { get; private set; }

    public PlacementInputGate(AutoBattleStateController state)
    {
        state.OnPreparationPhase += () => IsEnabled = true;
        state.OnBattlePhase += () => IsEnabled = false;
        state.OnResolutionPhase += () => IsEnabled = false;
        state.OnPause += () => IsEnabled = false;
        state.OnVictory += () => IsEnabled = false;
        state.OnDefeat += () => IsEnabled = false;
        state.OnSetup += () => IsEnabled = false;
        state.OnStarting += () => IsEnabled = false;
    }
}