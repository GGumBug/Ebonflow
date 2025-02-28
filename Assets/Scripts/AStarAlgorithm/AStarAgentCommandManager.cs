using UnityEngine;
using System.Collections.Generic;

public class AStarAgentCommandManager : Singleton<AStarAgentCommandManager>
{
    [SerializeField] private AStarAgent[] _enemies;

    [SerializeField] private HashSet<AStarAgent> allyUnits;
    [SerializeField] private HashSet<AStarAgent> enemyUnits;

    private AStarAlgorithmManager _aStarAlgorithmManager;

    private void Awake() 
    {
        _aStarAlgorithmManager = AStarAlgorithmManager.Instance;
    }

    public List<AStarNode> FindNearestEnemy(AStarAgent startAgent, TeamType team)
    {
        HashSet<AStarAgent> targetUnits = team == TeamType.Ally ? GetEnemyHashSet() : allyUnits;

        return _aStarAlgorithmManager.GetPath(startAgent, targetUnits);
    }

    HashSet<AStarAgent> GetEnemyHashSet()
    {
        HashSet<AStarAgent> newHashSet = new HashSet<AStarAgent>();

        foreach (var enemy in _enemies)
            newHashSet.Add(enemy);

        return newHashSet;
    }
}
