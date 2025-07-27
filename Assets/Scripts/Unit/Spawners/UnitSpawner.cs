using AutoBattle;
using System;
using UnityEngine;

public interface IUnitSpawner
{
    Unit Spawn(int unitId, int starLevel, TeamType team, Vector2Int position, IGridManager gridManager);
}

public class UnitSpawner : IUnitSpawner
{
    private readonly GameObject _prefab;
    private readonly Transform _allyContainer;
    private readonly Transform _enemyContainer;
    private readonly IBattleRoster _roster;                  // 추가
    private readonly IGridManager _battleGrid;              // (선택) BattleGrid 캐시

    private event Action<Unit> OnUnitDied;
    private event Func<int, int, UnitAggregate> OnRequestUnitStatData;

    public UnitSpawner(
        GameObject prefab,
        Transform allyContainer,
        Transform enemyContainer,
        Func<int, int, UnitAggregate> onRequestUnitStatData,
        Action<Unit> onUnitDied,
        IBattleRoster roster,
        IGridManager battleGrid   // 또는 AStarGrid
    )
    {
        _prefab = prefab;
        _allyContainer = allyContainer;
        _enemyContainer = enemyContainer;
        OnRequestUnitStatData = onRequestUnitStatData;
        OnUnitDied = onUnitDied;
        _roster = roster;
        _battleGrid = battleGrid;
    }

    public Unit Spawn(int unitId, int starLevel, TeamType team, Vector2Int pos, IGridManager gridManager)
    {
        Vector3 spawnPos = new Vector3(pos.x, pos.y, 0);
        Transform container = team == TeamType.Ally ? _allyContainer : _enemyContainer;
        var unit = PoolManager.Instance.GetFromPool<Unit>(_prefab, container, spawnPos, Quaternion.identity);
        unit.Setup(team, OnRequestUnitStatData.Invoke(unitId, starLevel).Stat, gridManager);
        unit.OnDied += OnUnitDied;

        // BattleGrid라면 등록
        if (gridManager == _battleGrid && !_roster.Contains(unit))
        {
            _roster.Register(unit);
        }

        return unit;
    }
}
