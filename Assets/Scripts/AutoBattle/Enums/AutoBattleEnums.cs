namespace AutoBattle
{
    public enum AutoBattleGameState
    {
        Setup,
        Starting,
        PreparationPhase,
        BattlePhase,
        ResolutionPhase,
        Pause,
        Victory,
        Defeat
    }

    public enum GridType
    {
        Bench,
        Battle
    }
}