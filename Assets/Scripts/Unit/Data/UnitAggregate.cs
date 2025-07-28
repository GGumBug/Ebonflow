using UnityEngine;

public class UnitAggregate
{
    public UnitData Data { get; }
    public UnitStatData Stat { get; }
    public int Price { get; }

    public UnitAggregate(UnitData data, UnitStatData stat)
    {
        Data = data;
        Stat = stat;

        if (data != null && stat != null)
        {
            int unitPrice = (int)data.UnitTier;
            int exponent = stat.StarLevel - 1;
            int unitCount = (int)Mathf.Pow(3, exponent);
            Price = unitPrice * unitCount;
        }
    }
}
