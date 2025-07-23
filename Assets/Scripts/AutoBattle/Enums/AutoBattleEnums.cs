namespace AutoBattle
{
    public enum AutoBattleGameState
    {
        Setup,
        Starting,
        PreparationPhase,
        BattlePhase,
        Victory,
        Defeat,
        ResolutionPhase,
        Pause,
    }

    public enum GridType
    {
        Bench,
        Battle
    }
}