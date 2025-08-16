using AutoBattle;
using AutoBattle.Input;
using StageEditor;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitSpawner
{
    private readonly GameObject _prefab;
    private readonly Transform _allyContainer;
    private readonly Transform _enemyContainer;
    private readonly IBattleRoster _roster;                  // 추가
    private readonly IGridManager _battleGrid;              // (선택) BattleGrid 캐시
    private readonly IGridManager _benchGrid;              // (선택) BattleGrid 캐시
    private readonly AutoBattleDataManager _autoBattleDataManager;

    private event Action<Unit> OnUnitDied;
    private event Func<int, int, UnitAggregate> OnRequestUnitStatData;

    public UnitSpawner(
        GameObject prefab,
        Transform allyContainer,
        Transform enemyContainer,
        Func<int, int, UnitAggregate> onRequestUnitStatData,
        Action<Unit> onUnitDied,
        IBattleRoster roster,
        IGridManager battleGrid,   // 또는 AStarGrid
        IGridManager benchGrid,
        List<StageEditorUnitInfo> enemyList
    )
    {
        _prefab = prefab;
        _allyContainer = allyContainer;
        _enemyContainer = enemyContainer;
        OnRequestUnitStatData = onRequestUnitStatData;
        OnUnitDied = onUnitDied;
        _roster = roster;
        _battleGrid = battleGrid;
        _benchGrid = benchGrid;
        _autoBattleDataManager = AutoBattleDataManager.Instance;
        SpawnEnemys(enemyList);
    }

    public void SpawnEnemys(List<StageEditorUnitInfo> enemyList)
    {
        foreach (var data in enemyList)
        {
            Unit newEnemy = Spawn(data.unitID, data.starLevel, TeamType.Enemy, new Vector2Int(data.gridX, data.gridY), _battleGrid);
            newEnemy.Agent.ReserveCurrentGridCell();
        }
    }

    public Unit Spawn(int unitId, int starLevel, TeamType team, Vector2Int pos, IGridManager gridManager)
    {
        Vector3 spawnPos = new Vector3(pos.x, pos.y, 0);
        Transform container = team == TeamType.Ally ? _allyContainer : _enemyContainer;
        var unit = PoolManager.Instance.GetFromPool<Unit>(_prefab, container, spawnPos, Quaternion.identity);
        unit.Setup(team, OnRequestUnitStatData.Invoke(unitId, starLevel), gridManager);
        unit.OnDied += OnUnitDied;

        // BattleGrid라면 등록
        if (gridManager == _battleGrid && !_roster.Contains(unit))
        {
            _roster.Register(unit);
        }

        return unit;
    }

    /// <summary>
    /// 세이브된 보유/배치 데이터를 기반으로 아군 유닛을 스폰하고 배치합니다.
    /// </summary>
    public void SpawnAlliesFromSave()
    {
        var ctx = _autoBattleDataManager.AutoBattlePlayerDataContext;
        if (ctx == null)
            return;

        if (ctx.OwnedUnits == null)
            return;

        var bench = _benchGrid as UnitBench;
        var battle = _battleGrid as AStarGrid;

        foreach (var rec in ctx.OwnedUnits) // IReadOnlyList<PlayerUnitRecord>
        {
            // 1) 배치 정보 조회(없으면 None)
            UnitPlacementRecord plc;
            bool hasPlacement = ctx.TryGetPlacement(rec.instanceId, out plc);
            var grid = plc.grid;

            Vector2Int pos;
            IGridManager targetGrid;

            if (grid == GridType.Battle && _battleGrid != null)
            {
                // 2) 배틀 그리드: 유효성/점유 체크 및 폴백
                pos = new Vector2Int(plc.x, plc.y);

                if (!_battleGrid.IsValidCell(pos) || _battleGrid.IsCellOccupied(pos))
                {
                    if (!TryFindNearestFreeBattleCell(pos, out pos))
                    {
                        Debug.LogWarning($"배틀 폴백 실패: instanceId={rec.instanceId} → 벤치로 스폰");
                        grid = GridType.Bench; // 벤치로 폴백
                    }
                }

                targetGrid = (grid == GridType.Battle) ? _battleGrid : _benchGrid;
                if (targetGrid == _benchGrid)
                {
                    int slot = bench.FirstEmptyIndex();
                    if (slot < 0) { Debug.LogWarning($"벤치가 가득 찼습니다. instanceId={rec.instanceId} 스폰 생략"); continue; }
                    pos = bench.GetBenchCell(slot);
                }
            }
            else
            {
                // 3) 벤치/None → 벤치 우선 배치
                int slot = (grid == GridType.Bench && bench.IsValidIndex(plc.x) && bench.IsEmpty(plc.x))
                             ? plc.x
                             : bench.FirstEmptyIndex();
                if (slot < 0) { Debug.LogWarning($"벤치가 가득 찼습니다. instanceId={rec.instanceId} 스폰 생략"); continue; }

                pos = bench.GetBenchCell(slot);
                targetGrid = _benchGrid;
            }

            // 4) 스폰 + 인스턴스ID 부여
            var unit = Spawn(rec.unitId, rec.starLevel, TeamType.Ally, pos, targetGrid);
            unit.SetInstanceId(rec.instanceId);

            // 5) 실제 그리드에 반영
            if (targetGrid == _battleGrid && _battleGrid is AStarGrid)
            {
                var draggable = unit.GetComponent<IUnitDraggable>();
                if (draggable != null)
                {
                    unit.Agent.ReserveCurrentGridCell();
                    unit.RegisterPlacement(grid);
                    battle.SyncRosterOnPlace(unit);
                }
            }
            else if (targetGrid == _benchGrid)
            {
                bench.TryPlace(unit, pos.x);
            }
        }
    }

    /// <summary>원하는 배틀 좌표가 막혀 있을 때 주변에서 가장 가까운 빈 칸을 탐색</summary>
    private bool TryFindNearestFreeBattleCell(Vector2Int prefer, out Vector2Int found)
    {
        const int MAX_RADIUS = 6; // 맵 크기에 맞춰 조절
        for (int r = 0; r <= MAX_RADIUS; r++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                int dy = r - Math.Abs(dx);
                // 링 스캔: (dx, ±dy)
                foreach (var p in new[] { new Vector2Int(prefer.x + dx, prefer.y + dy),
                                      new Vector2Int(prefer.x + dx, prefer.y - dy) })
                {
                    if (_battleGrid.IsValidCell(p) && !_battleGrid.IsCellOccupied(p))
                    {
                        found = p;
                        return true;
                    }
                }
            }
        }
        found = default;
        return false;
    }
}
