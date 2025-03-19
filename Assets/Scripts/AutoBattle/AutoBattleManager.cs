using UnityEngine;
using System.Collections.Generic;

public class AutoBattleManager : Singleton<AutoBattleManager>
{
    [SerializeField] Unit[] allyArray;
    [SerializeField] Unit[] enemyArray;

    private AStarAgentCommandManager _aStarAgentCommandManager;

    public HashSet<Unit> AllyUnits { get; private set; }
    public HashSet<Unit> EnemyUnits { get; private set; }
    public AutoBattleStateController StateController { get; private set; }

    private void Awake()
    {
        AllyUnits = new HashSet<Unit>(allyArray);
        EnemyUnits = new HashSet<Unit>(enemyArray);
        StateController = new AutoBattleStateController();

        _aStarAgentCommandManager = AStarAgentCommandManager.Instance;
        _aStarAgentCommandManager.OnRequestAllyUnits += () => { return AllyUnits; };
        _aStarAgentCommandManager.OnRequestEnemyUnits += () => { return EnemyUnits; };
    }

    public void Setup()
    {
        foreach (var unit in AllyUnits)
            _aStarAgentCommandManager.SetupAgent(unit.Agent);

        foreach (var unit in EnemyUnits)
            _aStarAgentCommandManager.SetupAgent(unit.Agent);
    }

    public void StartBattle()
    {
        StateController.GameState = AutoBattleGameState.InProgress;

        foreach (var unit in AllyUnits)
            _aStarAgentCommandManager.StartAgentPathFollowing(unit.Agent);

        foreach (var unit in EnemyUnits)
            _aStarAgentCommandManager.StartAgentPathFollowing(unit.Agent);
    }
}
