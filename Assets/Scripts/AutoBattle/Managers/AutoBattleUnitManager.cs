using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System;
using AutoBattle.Input;

namespace AutoBattle
{
    public class AutoBattleUnitManager : Singleton<AutoBattleUnitManager>
    {
        private const int BENCH_COUNT = 8;

        private GameObject _unitPrefab;
        private UnitBench _unitBench;
        private IUnitSpawner _spawner;
        private IUnitRepository _statRepository;
        private IPlacementInputGate _placementInputGate;
        private IPlacementService _placementService;
        private UnitDragController _dragController;

        public event Action<TeamType> OnTeamEliminated;

        public IUnitRepository UnitStatRepository => _statRepository;
        public HashSet<Unit> AllyUnits { get; private set; }
        public HashSet<Unit> EnemyUnits { get; private set; }

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

        public void Setup()
        {
            AutoBattleManager autoBattleManager = AutoBattleManager.Instance;

            Transform _allyContainer = new GameObject("AllyUnits").transform;
            Transform _enemyContainer = new GameObject("EnemyUnits").transform;
            _allyContainer.SetParent(transform, false);
            _enemyContainer.SetParent(transform, false);

            _statRepository = new UnitRepository();
            _spawner = new UnitSpawner(_unitPrefab, _allyContainer, _enemyContainer, UnitStatRepository.Get, HandleUnitDeath);
            _unitBench = new UnitBench(BENCH_COUNT);
            _placementInputGate = new PlacementInputGate(autoBattleManager.StateController);
            _placementService = new DefaultPlacementService(AStarAlgorithmManager.Instance.Grid, _unitBench);
            _dragController = new UnitDragController();

            AllyUnits = new HashSet<Unit>();
            EnemyUnits = new HashSet<Unit>();

            AStarAlgorithmManager.Instance.OnRequestAllyUnits += () => { return AllyUnits; };
            AStarAlgorithmManager.Instance.OnRequestEnemyUnits += () => { return EnemyUnits; };

            autoBattleManager.OnBattleStarted += StartAllUnitsBattle;

            _dragController.Setup(_placementInputGate, _placementService);
        }

        public Unit SpawnAlly(int unitId, int starLevel, Vector2Int pos)
        {
            var newAlly = _spawner.Spawn(unitId, starLevel, TeamType.Ally, pos);
            AllyUnits.Add(newAlly);
            return newAlly;
        }

        public Unit SpawnEnemy(int unitId, int starLevel, Vector2Int pos)
        {
            var newEnemy = _spawner.Spawn(unitId, starLevel, TeamType.Enemy, pos);
            EnemyUnits.Add(newEnemy);
            return newEnemy;
        }

        private void StartAllUnitsBattle()
        {
            foreach (var ally in AllyUnits)
                ally.StartBattle();

            foreach (var enemy in EnemyUnits)
                enemy.StartBattle();
        }

        public void SpawnToBench(int unitId, int starLevel)
        {
            // 1) 빈 슬롯 인덱스 조회
            int slotIndex = _unitBench.FirstEmptyIndex();
            if (slotIndex < 0)
            {
                Debug.LogWarning("벤치가 가득 찼습니다.");
                return;
            }

            // 2) 셀 좌표 얻기
            Vector2Int benchCell = _unitBench.GetBenchCell(slotIndex);

            // 3) 스폰
            Unit newUnit = _spawner.Spawn(
                unitId,
                starLevel,
                TeamType.Ally,   // 벤치는 아군 범주로 처리
                benchCell);
        }

        private void HandleUnitDeath(Unit unit)
        {
            var set = unit.GetTeam() == TeamType.Ally ? AllyUnits : EnemyUnits;
            set.Remove(unit);

            if (set.Count == 0)
                OnTeamEliminated?.Invoke(unit.GetTeam());
        }

        private void OnDisable()
        {
            _dragController.OnDisableEvents();
        }
    }
}