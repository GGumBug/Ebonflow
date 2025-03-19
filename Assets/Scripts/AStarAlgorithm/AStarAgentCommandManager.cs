using UnityEngine;
using System.Collections.Generic;
using System;

public class AStarAgentCommandManager : Singleton<AStarAgentCommandManager>
{
    [SerializeField] private bool allowDiagonal;
    [SerializeField] private bool dontCrossCorner;

    private AStarAlgorithmManager _aStarAlgorithmManager;

    public Func<HashSet<Unit>> OnRequestAllyUnits;
    public Func<HashSet<Unit>> OnRequestEnemyUnits;

    private bool GetAllowDiagonal() { return allowDiagonal; }
    private bool GetDontCrossCorner() { return dontCrossCorner; }

    private void Awake() 
    {
        _aStarAlgorithmManager = AStarAlgorithmManager.Instance;
    }

    public void SetupAgent(AStarAgent agent)
    {
        agent.LockCurrentGridPositionWithSettings(GetAllowDiagonal, GetDontCrossCorner, FindNearestEnemy);
    }

    private List<AStarNode> FindNearestEnemy(AStarAgent startAgent, bool allowDiagonal = false, bool dontCrossCorner = false)
    {
        HashSet<Unit> targetUnits = startAgent.GetTeam() == TeamType.Ally ? OnRequestEnemyUnits.Invoke() : OnRequestAllyUnits.Invoke();

        return _aStarAlgorithmManager.GetPath(startAgent, targetUnits, allowDiagonal, dontCrossCorner);
    }

    /// <summary>
    /// 주어진 에이전트의 가장 가까운 적 경로를 찾고, 그 경로를 따라 이동을 시작합니다.
    /// 경로가 없으면 경로 재탐색 또는 오류 로그를 남깁니다.
    /// </summary>
    /// <param name="agent">경로 탐색을 수행할 AStarAgent</param>
    public void StartAgentPathFollowing(AStarAgent agent)
    {
        List<AStarNode> currentPath = FindNearestEnemy(agent, allowDiagonal, dontCrossCorner);

        if (currentPath == null || currentPath.Count == 0)
        {
            Debug.LogWarning($"StartAgentPathFollowing: {agent.name}에 대해 유효한 경로를 찾지 못했습니다.");
            return;
        }

        agent.SetCurrentPath(currentPath);
        agent.BeginPathFollowing();
    }
}