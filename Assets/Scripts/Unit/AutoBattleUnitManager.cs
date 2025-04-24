using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

public class AutoBattleUnitManager : Singleton<AutoBattleUnitManager>
{
    private Transform _allyContainer;
    private Transform _enemyContainer;
    private AStarAlgorithmManager _aStarAlgorithmManager;
    private GameObject _unitPrefab;
    private IUnitSpawner _spawner;
    private IUnitStatRepository _statRepository;

    public Transform AllyContainer => _allyContainer;
    public Transform EnemyContainer => _enemyContainer;
    public IUnitStatRepository UnitStatRepository => _statRepository;
    public HashSet<Unit> AllyUnits { get; private set; }
    public HashSet<Unit> EnemyUnits { get; private set; }

    public async UniTask LoadAsset()
    {
        _unitPrefab = await AddressableManager.Instance
            .Load<GameObject>(AddressableKey.AutoBattleUnitPrefab.ToString());

        if (_unitPrefab == null)
        {
            Debug.LogError("AutoBattleUnitPrefab을 로드하지 못했습니다.");
            return;
        }
    }

    public void Setup()
    {
        _statRepository = new UnitStatRepository();
        _spawner = new UnitSpawner(_unitPrefab, AllyContainer, UnitStatRepository.GetUnitStatData);

        _allyContainer  = new GameObject("AllyUnits").transform;
        _enemyContainer = new GameObject("EnemyUnits").transform;
        _allyContainer .SetParent(transform, false);
        _enemyContainer.SetParent(transform, false);

        AllyUnits = new HashSet<Unit>();
        EnemyUnits = new HashSet<Unit>();

        _aStarAlgorithmManager = AStarAlgorithmManager.Instance;
        _aStarAlgorithmManager.OnRequestAllyUnits += () => { return AllyUnits; };
        _aStarAlgorithmManager.OnRequestEnemyUnits += () => { return EnemyUnits; };

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
}
