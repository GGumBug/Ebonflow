using System;

[Serializable]
public class UnitStatData
{
    public int UnitId { get; private set; }

    public int StarLevel { get; private set; }

    public int Health { get; private set; }

    public float Attack { get; private set; }

    public UnitStatData(DB_UnitStats unitStats)
    {
        if (unitStats == null)
            throw new ArgumentNullException(nameof(unitStats), "DB_UnitStats 인스턴스가 null입니다.");

        UnitId = unitStats.f_UnitId;
        StarLevel = unitStats.f_StarLevel;
        Health = unitStats.f_Health;
        Attack = unitStats.f_Attack;
    }
}