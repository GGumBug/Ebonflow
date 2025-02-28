using System.Collections.Generic;
using UnityEditor.ShaderGraph;
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
        AStarAlgorithmManager.Instance.CreateGridFromTilemap();
        _currentPath = AStarAgentCommandManager.Instance.FindNearestEnemy(this, team, true, true);
        _isMove = true;
    }

    private void Move()
    {
        if (_currentPathIndex < _currentPath.Count - 1)
        {
            var currentNode = _currentPath[_currentPathIndex];
            Vector2Int destPos = new Vector2Int(currentNode.X, currentNode.Y);

            transform.position = Vector2.MoveTowards(transform.position, destPos, moveSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, destPos) < 0.1f)
            {
                transform.position = new Vector2(destPos.x, destPos.y);
                _currentPathIndex++;
            }
        }
        else if (_currentPathIndex >= _currentPath.Count - 1)
        {
            _isMove = false;
            _currentPath = null;
            _currentPathIndex = 0;
        }
    }

    private bool IsDrawLine => _currentPath != null && _currentPath.Count > 0;
    private void OnDrawGizmos()
    {
        if (IsDrawLine)
        {
            for (int i = 0; i < _currentPath.Count - 1; i++)
            {
                Vector2 from = new Vector2(_currentPath[i].X, _currentPath[i].Y);
                Vector2 to = new Vector2(_currentPath[i + 1].X, _currentPath[i + 1].Y);
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(from, to);
            }
        }
    }
}
