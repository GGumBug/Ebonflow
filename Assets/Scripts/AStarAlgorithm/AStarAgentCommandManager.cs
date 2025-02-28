using UnityEngine;
using System.Collections.Generic;

public class AStarAgentCommandManager : Singleton<AStarAgentCommandManager>
{
    [SerializeField] private HashSet<AStarAgent> allyUnits;
    [SerializeField] private HashSet<AStarAgent> enemyUnits;

    private AStarAlgorithmManager _aStarAlgorithmManager;

    private void Awake() 
    {
        _aStarAlgorithmManager = AStarAlgorithmManager.Instance;
    }

    public List<AStarNode> FindNearestEnemy(AStarAgent startAgent, TeamType team)
    {
        HashSet<AStarAgent> targetUnits = team == TeamType.Ally ? enemyUnits : allyUnits;

        return _aStarAlgorithmManager.GetPath(startAgent, targetUnits);
    }
}
