using AutoBattle.Input;
using AutoBattle.UI;
using Cysharp.Threading.Tasks;
using StageEditor;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AutoBattle
{
    public class AutoBattleUnitManager : Singleton<AutoBattleUnitManager>
    {
        private GameObject              _unitPrefab;
        private UnitSpawner             _spawner;
        private IUnitRepository         _statRepository;
        private IPlacementInputGate     _placementInputGate;
        private IPlacementService       _placementService;

        public event Action<TeamType>   OnTeamEliminated;

        public IUnitRepository          UnitStatRepository => _statRepository;
        public BattleRoster             Roster { get; private set; }
        public UnitBench                UnitBench { get; private set; }
        public UnitDragController       DragController { get; private set; }

        public async UniTask LoadAsset()
        {
            _unitPrefab = await AddressableManager.Instance
                .Load<GameObject>(AddressableKey.AutoBattleUnitPrefab);

            if (_unitPrefab == null)
            {
                Debug.LogError("AutoBattleUnitPrefab을 로드하지 못했습니다.");
                return;
            }
        }

        public void Setup(IGridManager battleGrid, SellZonePanel sellZonePanel, List<StageEditorUnitInfo> enemyList)
        {
            AutoBattleManager autoBattleManager = AutoBattleManager.Instance;
            Roster = new BattleRoster();
            Transform _allyContainer = new GameObject("AllyUnits").transform;
            Transform _enemyContainer = new GameObject("EnemyUnits").transform;
            _allyContainer.SetParent(transform, false);
            _enemyContainer.SetParent(transform, false);

            _statRepository = new UnitRepository();
            _spawner = new UnitSpawner(_unitPrefab, _allyContainer, _enemyContainer, UnitStatRepository.Get, HandleUnitDeath, Roster, battleGrid, enemyList);

            UnitBench = gameObject.AddComponent<UnitBench>();
            _placementInputGate = new PlacementInputGate(autoBattleManager.StateController);
            _placementService = new DefaultPlacementService(AStarAlgorithmManager.Instance.Grid, UnitBench);
            DragController = gameObject.AddComponent<UnitDragController>();

            AStarAlgorithmManager.Instance.OnRequestAllyUnits += () => { return Roster.Allies; };
            AStarAlgorithmManager.Instance.OnRequestEnemyUnits += () => { return Roster.Enemies; };

            DragController.Setup(_placementInputGate, _placementService, sellZonePanel);

            autoBattleManager.StateController.BattleEntered.Add(CheckInitialTeamStatus, 0);
        }

        public Unit SpawnAlly(int unitId, int starLevel, Vector2Int pos, IGridManager gridManager)
        {
            var newAlly = _spawner.Spawn(unitId, starLevel, TeamType.Ally, pos, gridManager);
            return newAlly;
        }

        public Unit SpawnEnemy(int unitId, int starLevel, Vector2Int pos, IGridManager gridManager)
        {
            var newEnemy = _spawner.Spawn(unitId, starLevel, TeamType.Enemy, pos, gridManager);
            return newEnemy;
        }

        public bool SpawnToBench(int unitId, int starLevel)
        {
            // 1) 빈 슬롯 인덱스 조회
            int slotIndex = UnitBench.FirstEmptyIndex();
            if (slotIndex < 0)
            {
                Debug.LogWarning("벤치가 가득 찼습니다.");
                return false;
            }

            // 2) 셀 좌표 얻기
            Vector2Int benchCell = UnitBench.GetBenchCell(slotIndex);

            // 3) 스폰
            Unit newUnit = _spawner.Spawn(
                unitId,
                starLevel,
                TeamType.Ally,   // 벤치는 아군 범주로 처리
                benchCell,
                UnitBench
                );

            UnitBench.TryPlaceFirstEmpty(newUnit, out slotIndex);
            newUnit.SetCurrentGridPosition(benchCell);
            return true;
        }

        private void CheckInitialTeamStatus()
        {
            int allyCount = Roster.Allies.Count;
            int enemyCount = Roster.Enemies.Count;

            if (allyCount == 0 || enemyCount == 0)
            {
                bool victory = enemyCount == 0;
                var eliminatedTeam = victory ? TeamType.Enemy : TeamType.Ally;
                OnTeamEliminated?.Invoke(eliminatedTeam);
            }
        }

        private void HandleUnitDeath(Unit unit)
        {
            var set = unit.GetTeam() == TeamType.Ally ? Roster.Allies : Roster.Enemies;
            Roster.Unregister(unit);

            if (set.Count == 0)
                OnTeamEliminated?.Invoke(unit.GetTeam());
        }
    }
}