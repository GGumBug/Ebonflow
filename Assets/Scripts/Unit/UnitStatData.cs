using System;

[Serializable]
public struct UnitStatData
{
    public readonly int unitID;
    public readonly int starLevel;
    public readonly int health;
    public readonly float attack;

    public UnitStatData(DB_UnitStats unitStats)
    {
        if (unitStats == null)
            throw new ArgumentNullException(nameof(unitStats), "DB_UnitStats 인스턴스가 null입니다.");

        this.unitID = unitStats.f_UnitId;
        this.starLevel = unitStats.f_StarLevel;
        this.health = unitStats.f_Health;
        this.attack = unitStats.f_Attack;
    }
}