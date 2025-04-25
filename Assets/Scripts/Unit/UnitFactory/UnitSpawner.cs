using UnityEngine;
using System;

public interface IUnitSpawner
{
    Unit Spawn(int unitId, int starLevel, TeamType team, Vector2Int position);
}

public class UnitSpawner : IUnitSpawner
{
    private readonly GameObject _prefab;
    private readonly Transform  _container;
    private event Func<int, int, UnitStatData> OnRequestUnitStatData;

    public UnitSpawner(GameObject prefab, Transform container, Func<int, int, UnitStatData> onRequestUnitStatData)
    {        
        _prefab = prefab;
        _container = container;
        OnRequestUnitStatData = onRequestUnitStatData;
    }

    public Unit Spawn(int unitId, int starLevel, TeamType team, Vector2Int pos)
    {
        Vector3 spawnPos = new Vector3(pos.x, pos.y, 0);
        GameObject go = UnityEngine.Object.Instantiate(_prefab, spawnPos, Quaternion.identity, _container);
        var unit = go.GetComponent<Unit>();

        unit.Setup(team, OnRequestUnitStatData.Invoke(unitId, starLevel));
        return unit;
    }
}