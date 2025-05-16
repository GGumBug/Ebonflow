public struct AutoBattleSceneContext
{
    public int StageNumber { get; }
    public int Floor { get; }
    public int LocationTypeId { get; }

    public AutoBattleSceneContext(int stageNumber, int floor, int locationTypeId)
    {
        StageNumber = stageNumber;
        Floor = floor;
        LocationTypeId = locationTypeId;
    }
}
