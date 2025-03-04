using UnityEngine;
using System.Collections.Generic;

public class AStarAgentCommandManager : Singleton<AStarAgentCommandManager>
{
    [SerializeField] private AStarAgent[] allys;
    [SerializeField] private AStarAgent[] enemies;

    // [SerializeField] private HashSet<AStarAgent> allyUnits;
    // [SerializeField] private HashSet<AStarAgent> enemyUnits;

    private AStarAlgorithmManager _aStarAlgorithmManager;

    private void Awake() 
    {
        _aStarAlgorithmManager = AStarAlgorithmManager.Instance;
    }

    private void Start()
    {
        foreach (var agent in allys)
        {
            agent.MarkCurrentPositionAsBlocked();
        }

        foreach (var agent in enemies)
        {
            agent.MarkCurrentPositionAsBlocked();
        }

        Debug.Log($"Agent 위치에 IsBlock 설정");

        foreach (var agent in allys)
        {
            agent.FollowPath();
        }

        foreach (var agent in enemies)
        {
            agent.FollowPath();
        }
    }

    public List<AStarNode> FindNearestEnemy(AStarAgent startAgent, TeamType team, bool allowDiagonal = false, bool dontCrossCorner = false)
    {
        HashSet<AStarAgent> targetUnits = team == TeamType.Ally ? GetEnemyHashSet(enemies) : GetEnemyHashSet(allys);

        return _aStarAlgorithmManager.GetPath(startAgent, targetUnits, allowDiagonal, dontCrossCorner);
    }

    HashSet<AStarAgent> GetEnemyHashSet(AStarAgent[] aStarAgentArray)
    {
        HashSet<AStarAgent> newHashSet = new HashSet<AStarAgent>();

        foreach (var agent in aStarAgentArray)
            newHashSet.Add(agent);

        return newHashSet;
    }
}