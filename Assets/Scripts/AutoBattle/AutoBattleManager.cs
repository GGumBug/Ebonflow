using UnityEngine;
using System.Collections.Generic;

public class AutoBattleManager : Singleton<AutoBattleManager>
{
    [SerializeField] Unit[] allyArray;
    [SerializeField] Unit[] enemyArray;

    private AStarAlgorithmManager _aStarAlgorithmManager;
    private DamageCalculator _damageCalculator;

    public HashSet<Unit> AllyUnits { get; private set; }
    public HashSet<Unit> EnemyUnits { get; private set; }
    public AutoBattleStateController StateController { get; private set; }

    private void Awake()
    {
        _damageCalculator = new DamageCalculator();

        AllyUnits = new HashSet<Unit>(allyArray);
        EnemyUnits = new HashSet<Unit>(enemyArray);
        StateController = new AutoBattleStateController();

        _aStarAlgorithmManager = AStarAlgorithmManager.Instance;
        _aStarAlgorithmManager.OnRequestAllyUnits += () => { return AllyUnits; };
        _aStarAlgorithmManager.OnRequestEnemyUnits += () => { return EnemyUnits; };
    }

    public void Setup()
    {
        foreach (var ally in AllyUnits)
            ally.Setup(0, 1);

        foreach (var enemy in EnemyUnits)
            enemy.Setup(1, 1);
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
