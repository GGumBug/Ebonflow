using System.Collections.Generic;
using UnityEngine;

public class AStarAgent : MonoBehaviour, IAStarPathPoint, IAStarPathFollower
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float stepDelay = 0.5f;

    private AStarAlgorithmManager _aStarAlgorithmManager;
    private List<AStarNode> _currentPath;
    private int _currentPathIndex = 0;

    public Vector2Int PathPoint => new Vector2Int(
        Mathf.RoundToInt(transform.position.x),
        Mathf.RoundToInt(transform.position.y)
    );

    private void Awake() 
    {
        _aStarAlgorithmManager = AStarAlgorithmManager.Instance;
    }

    public void FollowPath()
    {
        
    }
}
