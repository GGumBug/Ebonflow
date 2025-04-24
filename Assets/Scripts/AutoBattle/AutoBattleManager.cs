using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

public class AutoBattleManager : Singleton<AutoBattleManager>
{
    private AStarAlgorithmManager _aStarAlgorithmManager;
    private IUnitSpawner _spawner;
    private DamageCalculator _damageCalculator;
    private Transform _allyContainer;
    private Transform _enemyContainer;

    public Transform AllyContainer => _allyContainer;
    public Transform EnemyContainer => _enemyContainer;
    public HashSet<Unit> AllyUnits { get; private set; }
    public HashSet<Unit> EnemyUnits { get; private set; }
    public AutoBattleStateController StateController { get; private set; }

    public async UniTask LoadAsset()
    {
        GameObject unitPrefab = await AddressableManager.Instance
            .Load<GameObject>(AddressableKey.AutoBattleUnitPrefab.ToString());

        if (unitPrefab == null)
        {
            Debug.LogError("AutoBattleUnitPrefab을 로드하지 못했습니다.");
            return;
        }

        _spawner = new UnitSpawner(unitPrefab, AllyContainer);
    }

    public void Setup()
    {
        _allyContainer  = new GameObject("AllyUnits").transform;
        _enemyContainer = new GameObject("EnemyUnits").transform;
        _allyContainer .SetParent(transform, false);
        _enemyContainer.SetParent(transform, false);
        
        _damageCalculator = new DamageCalculator();

        AllyUnits = new HashSet<Unit>();
        EnemyUnits = new HashSet<Unit>();
        StateController = new AutoBattleStateController();

        _aStarAlgorithmManager = AStarAlgorithmManager.Instance;
        _aStarAlgorithmManager.OnRequestAllyUnits += () => { return AllyUnits; };
        _aStarAlgorithmManager.OnRequestEnemyUnits += () => { return EnemyUnits; };
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
        AllyUnits.Add(newEnemy);
        return newEnemy;
    }
    
    public void StartBattle()
    {
        StateController.GameState = AutoBattleGameState.InProgress;

        foreach (var ally in AllyUnits)
            ally.StartBattle();

        foreach (var enemy in EnemyUnits)
            enemy.StartBattle();
    }

    public void Attack(Unit attacker, Unit defender)
    {
        //var atkStats = attacker.Stat.Data;
        //var defStats = defender.Stat.Data;

        //int damage = _damageCalculator.CalculateDamage(atkStats, defStats);
        //defender.ApplyDamage(damage);
    }
}
