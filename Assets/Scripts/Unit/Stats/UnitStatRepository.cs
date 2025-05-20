using System.Collections.Generic;

public class UnitStatRepository : IUnitStatRepository
{
    public UnitStatData GetUnitStatData(int unitId, int starLevel)
    {
        var entity = DB_UnitStats.FindEntity(f => f.f_UnitId == unitId && f.f_StarLevel == starLevel);
        if (entity == null)
            throw new KeyNotFoundException($"UnitStat not found for ID={unitId}, Star={starLevel}");
        return new UnitStatData(entity);
    }
}