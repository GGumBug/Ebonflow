using System.Collections.Generic;
using System.Linq;

public class UnitRepository : IUnitRepository
{
    private readonly Dictionary<int, UnitData> _dataMap;
    private readonly Dictionary<(int, int), UnitStatData> _statMap;

    public UnitRepository()
    {
        _dataMap = new();
        _statMap = new();

        var allUnitEntities = DB_Units.FindEntities(e => true);

        if (allUnitEntities == null || allUnitEntities.Count <= 0)
            throw new System.Exception("No UnitData");

        foreach (var unitData in allUnitEntities)
        {
            UnitData newUnitData = new UnitData(unitData);
            _dataMap.Add(newUnitData.UnitId, newUnitData);
        }

        var allUnitStatsEntities = DB_UnitStats.FindEntities(e => true);
        if (allUnitStatsEntities == null || allUnitStatsEntities.Count <= 0)
            throw new System.Exception("No UnitStats Data");

        foreach (var statData in allUnitStatsEntities)
        {
            UnitStatData unitStatData = new UnitStatData(statData);
            _statMap.Add((unitStatData.UnitId, unitStatData.StarLevel), unitStatData);
        }
    }

    public UnitAggregate Get(int unitId, int starLevel)
    {
        if (!_dataMap.TryGetValue(unitId, out var data))
            throw new KeyNotFoundException($"ID={unitId}에 대한 UnitData를 찾을 수 없습니다.");

        if (!_statMap.TryGetValue((unitId, starLevel), out var stat))
            throw new KeyNotFoundException($"ID={unitId}, Star={starLevel}에 대한 UnitStatData를 찾을 수 없습니다.");

        return new UnitAggregate(data, stat);
    }

    public bool Exists(int unitId)
        => _dataMap.ContainsKey(unitId);

    public int GetMaxId()
        => _dataMap.Keys.Max();
}
