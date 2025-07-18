public class UnitAggregate
{
    public UnitData Data { get; }
    public UnitStatData Stat { get; }

    public UnitAggregate(UnitData data, UnitStatData stat)
    {
        Data = data;
        Stat = stat;
    }
}
