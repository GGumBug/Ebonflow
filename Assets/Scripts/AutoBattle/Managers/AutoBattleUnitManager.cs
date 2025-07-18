using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace AutoBattle
{
    public class AutoBattleUnitManager : Singleton<AutoBattleUnitManager>
    {
        private GameObject _unitPrefab;
        private IUnitSpawner _spawner;
        private IUnitRepository _statRepository;

        public event Action<TeamType> OnTeamEliminated;

        public IUnitRepository UnitStatRepository => _statRepository;
        public HashSet<Unit> AllyUnits { get; private set; }
        public HashSet<Unit> EnemyUnits { get; private set; }

        public async UniTask LoadAsset()
        {
            _unitPrefab = await AddressableManager.Instance
                .Load<GameObject>(AddressableKeyExtensions.ToKey(AddressableKey.AutoBattleUnitPrefab));

            if (_unitPrefab == null)
            {
                Debug.LogError("AutoBattleUnitPrefab을 로드하지 못했습니다.");
                return;
            }
        }

        public void Setup()
        {
            Transform _allyContainer = new GameObject("AllyUnits").transform;
            Transform _enemyContainer = new GameObject("EnemyUnits").transform;
            _allyContainer.SetParent(transform, false);
            _enemyContainer.SetParent(transform, false);

            _statRepository = new UnitRepository();
            _spawner = new UnitSpawner(_unitPrefab, _allyContainer, _enemyContainer, UnitStatRepository.Get, HandleUnitDeath);

            AllyUnits = new HashSet<Unit>();
            EnemyUnits = new HashSet<Unit>();

            AStarAlgorithmManager.Instance.OnRequestAllyUnits += () => { return AllyUnits; };
            AStarAlgorithmManager.Instance.OnRequestEnemyUnits += () => { return EnemyUnits; };

            AutoBattleManager.Instance.OnBattleStarted += StartAllUnitsBattle;
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

        private void HandleUnitDeath(Unit unit)
        {
            var set = unit.GetTeam() == TeamType.Ally ? AllyUnits : EnemyUnits;
            set.Remove(unit);

            if (set.Count == 0)
                OnTeamEliminated?.Invoke(unit.GetTeam());
        }
    }
}