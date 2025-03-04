using System;
using System.Collections.Generic;
using UnityEngine;

public class AStarAgent : MonoBehaviour, IAStarPathPoint, IAStarPathFollower
{
    [Header("Team Settings")]
    [Tooltip("이 에이전트의 팀 타입 (예: 아군 또는 적군)")]
    [SerializeField] private TeamType team;

    [Header("Movement Settings")]
    [Tooltip("초당 이동 속도 (단위: 유닛)")]
    [SerializeField] private float moveSpeed = 6f;
    [Tooltip("각 타일로 이동할 때 적용되는 지연 시간 (초 단위)")]
    [SerializeField] private float stepDelay = 0.5f;

    [Header("Debug Settings")]
    [Tooltip("경로를 Gizmos로 그릴지 여부 (디버깅용)")]
    [SerializeField] private bool isDrawLine;

    private bool _isMove;
    private float _currentStepDelay;
    private List<AStarNode> _currentPath;
    private int _currentPathIndex = 1;

    public event Func<bool> OnTargetTileOccupied;
    public event Func<bool> OnEnemyInRange;
    public event Action OnStepCompleted;

    /// <summary>
    /// 현재 경로에서 다음 노드가 존재하는지 여부
    /// </summary>
    private bool HasNextNode => _currentPathIndex < _currentPath.Count - 1;

    /// <summary>
    /// 현재 경로의 마지막 노드에 도달했는지 여부
    /// </summary>
    private bool IsAtEndOfPath => _currentPathIndex == _currentPath.Count - 1;

    public Vector2Int PathPoint => new Vector2Int(
        Mathf.RoundToInt(transform.position.x),
        Mathf.RoundToInt(transform.position.y)
    );

    private void Awake() 
    {
        _currentStepDelay = stepDelay;
    }

    private void Update() 
    {
        if (_isMove && _currentPath != null)
            ProcessMovement();
    }

    public void FollowPath()
    {
        _currentPath = AStarAgentCommandManager.Instance.FindNearestEnemy(this, team, true, true);
        _isMove = true;
    }

    private void ProcessMovement()
    {
        Vector2Int destPos = GetCurrentDestination();

        if (!IsAtEndOfPath && OnTargetTileOccupied.Invoke())
            HandleOccupiedTileResponse();

        if (HasNextNode)
            MoveTowardsNextNode(destPos);
        else if (IsAtEndOfPath)
            EndMovement();
    }

    private Vector2Int GetCurrentDestination()
    {
        AStarNode currentNode = _currentPath[_currentPathIndex];
        Vector2Int destPos = new Vector2Int(currentNode.X, currentNode.Y);
        return destPos;
    }

    private void MoveTowardsNextNode(Vector2Int destPos)
    {
        if (_currentStepDelay < stepDelay)
        {
            _currentStepDelay += Time.deltaTime;
            return;
        }

        transform.position = Vector2.MoveTowards(transform.position, destPos, moveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, destPos) < 0.1f)
        {
            SnapToDestinationAndAdvance(destPos);
        }
    }

    private void SnapToDestinationAndAdvance(Vector2Int destPos)
    {
        transform.position = new Vector2(destPos.x, destPos.y);
        _currentStepDelay = 0f;
        _currentPathIndex++;

        OnStepCompleted?.Invoke();
    }

    private void SnapToLastValidNode()
    {
        if (_currentPath[_currentPathIndex - 1] != null)
        {
            AStarNode prevNode = _currentPath[_currentPathIndex - 1];
            transform.position = new Vector2(prevNode.X, prevNode.Y);
        }
        else
        {
            AStarNode currentNode = _currentPath[_currentPathIndex];
            transform.position = new Vector2(currentNode.X, currentNode.Y);
        }
    }

    private void HandleOccupiedTileResponse()
    {
        if (OnEnemyInRange.Invoke())
        {
            SnapToLastValidNode();
            EndMovement();
            Debug.Log("공격 스테이트로 전환");
        }
        else
        {
            SnapToLastValidNode();
            RecalculatePath();
        }
    }

    private void RecalculatePath()
    {
        ClearFllowing();
        FollowPath();
    }

    private void EndMovement()
    {
        ClearFllowing();
        _currentStepDelay = stepDelay;
    }

    private void ClearFllowing()
    {
        _isMove = false;
        _currentPath = null;
        _currentPathIndex = 1;
    }

    private void OnDrawGizmos()
    {
        if (isDrawLine && _currentPath != null)
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
