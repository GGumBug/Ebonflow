using System;
using System.Collections.Generic;
using System.Linq;

public class UnitRepository : IUnitRepository
{
    private readonly List<DB_UnitStats> _allStats;

    public UnitRepository()
    {
        _allStats = DB_UnitStats.FindEntities(e => true)
            ?? throw new InvalidOperationException("UnitStat 엔티티가 없습니다.");
    }

    public bool ExistsUnitId(int unitId)
    {
        return _allStats.Any(stat => stat.f_UnitId == unitId);
    }

    public UnitStatData GetUnitStatData(int unitId, int starLevel)
    {
        var entity = _allStats.Find(f => f.f_UnitId == unitId && f.f_StarLevel == starLevel);
        if (entity == null)
            throw new KeyNotFoundException($"UnitStat not found for ID={unitId}, Star={starLevel}");
        return new UnitStatData(entity);
    }

    public int GetMaxUnitId()
    {
        return _allStats.Max(stat => stat.f_UnitId);
    }
}