using UnityEngine;
using System;

public interface IUnitSpawner
{
    Unit Spawn(int unitId, int starLevel, TeamType team, Vector2Int position, IGridManager gridManager);
}

public class UnitSpawner : IUnitSpawner
{
    private readonly GameObject                 _prefab;
    private readonly Transform                  _allyContainer;
    private readonly Transform                  _enemyContainer;
    private event Action<Unit>                  OnUnitDied;
    private event Func<int, int, UnitAggregate>  OnRequestUnitStatData;

    public UnitSpawner(GameObject prefab, Transform allyContainer, Transform enemyContainer, Func<int, int, UnitAggregate> onRequestUnitStatData, Action<Unit> onUnitDied)
    {        
        _prefab = prefab;
        _allyContainer = allyContainer;
        _enemyContainer = enemyContainer;
        OnRequestUnitStatData = onRequestUnitStatData;
        OnUnitDied = onUnitDied;
    }

    public Unit Spawn(int unitId, int starLevel, TeamType team, Vector2Int pos, IGridManager gridManager)
    {
        Vector3 spawnPos = new Vector3(pos.x, pos.y, 0);
        Transform container = team == TeamType.Ally ? _allyContainer : _enemyContainer;
        GameObject go = UnityEngine.Object.Instantiate(_prefab, spawnPos, Quaternion.identity, container);
        var unit = go.GetComponent<Unit>();

        unit.Setup(team, OnRequestUnitStatData.Invoke(unitId, starLevel).Stat, gridManager);
        unit.OnDied += OnUnitDied;
        return unit;
    }
}