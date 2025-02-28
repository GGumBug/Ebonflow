using System.Collections.Generic;
using UnityEngine;

public class AStarAgent : MonoBehaviour, IAStarPathPoint, IAStarPathFollower
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float stepDelay = 0.5f;

    private bool _isMove;
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

    private void Update() 
    {
        if (_currentPath != null && _isMove)
            Move();
    }

    public void FollowPath(TeamType team)
    {
        _currentPath = AStarAgentCommandManager.Instance.FindNearestEnemy(this, team);
        _isMove = true;
    }

    private void Move()
    {
        if (_currentPathIndex < _currentPath.Count)
        {
            var currentNode = _currentPath[_currentPathIndex];
            Vector2Int destPos = new Vector2Int(currentNode.X, currentNode.Y);

            transform.position = Vector2.MoveTowards(transform.position, destPos, moveSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, destPos) < 0.1f)
                _currentPathIndex++;
        }
        else if (_currentPathIndex >= _currentPath.Count)
        {
            _isMove = false;
            _currentPath = null;
            _currentPathIndex = 0;
        }        
    }
}
