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

        foreach (var agent in allys)
        {
            agent.MarkCurrentPositionAsBlocked();
            agent.OnTargetLost += FindNearestEnemy;
        }

        foreach (var agent in enemies)
        {
            agent.MarkCurrentPositionAsBlocked();
            agent.OnTargetLost += FindNearestEnemy;
        }
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
            StartAgentPathFollowing(agent);
        }

        foreach (var agent in enemies)
        {
            StartAgentPathFollowing(agent);
        }
    }

    private List<AStarNode> FindNearestEnemy(AStarAgent startAgent, bool allowDiagonal = false, bool dontCrossCorner = false)
    {
        HashSet<AStarAgent> targetUnits = startAgent.Team == TeamType.Ally ? GetEnemyHashSet(enemies) : GetEnemyHashSet(allys);

        return _aStarAlgorithmManager.GetPath(startAgent, targetUnits, allowDiagonal, dontCrossCorner);
    }

    /// <summary>
    /// 주어진 에이전트의 가장 가까운 적 경로를 찾고, 그 경로를 따라 이동을 시작합니다.
    /// 경로가 없으면 경로 재탐색 또는 오류 로그를 남깁니다.
    /// </summary>
    /// <param name="agent">경로 탐색을 수행할 AStarAgent</param>
    private void StartAgentPathFollowing(AStarAgent agent)
    {
        List<AStarNode> currentPath = FindNearestEnemy(agent);

        if (currentPath == null || currentPath.Count == 0)
        {
            Debug.LogWarning($"StartAgentPathFollowing: {agent.name}에 대해 유효한 경로를 찾지 못했습니다.");
            return;
        }

        agent.SetCurrentPath(currentPath);
        agent.BeginPathFollowing();
    }



    HashSet<AStarAgent> GetEnemyHashSet(AStarAgent[] aStarAgentArray)
    {
        HashSet<AStarAgent> newHashSet = new HashSet<AStarAgent>();

        foreach (var agent in aStarAgentArray)
            newHashSet.Add(agent);

        return newHashSet;
    }
}