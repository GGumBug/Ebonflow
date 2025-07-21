using System;
using System.Collections.Generic;
using UnityEngine;

public class AStarAgent : MonoBehaviour, IAStarPathPoint, IAStarPathFollower
{
    [Header("Debug Settings")]
    [Tooltip("경로를 Gizmos로 그릴지 여부 (디버깅용)")]
    [SerializeField] private bool isDrawLine;

    private int _currentPathIndex = 1;
    private List<AStarNode> _currentPath;
    private AStarAlgorithmManager _aStarAlgorithmManager;
    private AStarGrid _grid;

    public Vector2Int CurrentGridPosition { get; private set; }
    public event Action<Vector2Int> OnMove;
    public event Func<TeamType> OnRequestTeamType;
    public event Action CrushOtherTeamAgent;
    public event Action OnPathCompleteAction;

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

    public Unit Unit => gameObject.GetComponent<Unit>();

    public TeamType GetTeam() => OnRequestTeamType.Invoke();

    private void Awake() 
    {
        _aStarAlgorithmManager = AStarAlgorithmManager.Instance;
        transform.position = (Vector3Int)PathPoint;
        CurrentGridPosition = PathPoint;

        
    }

    public void ReserveCurrentGridCell()
    {
        _grid ??= _aStarAlgorithmManager.Grid;
        _grid.SetNodeBlock(PathPoint, true, this);
    }

    public void UnreserveCurrentGridCell()
    {
        _grid ??= AStarAlgorithmManager.Instance.Grid;
        _grid.RemoveNodeBlock(PathPoint);
    }

    public void StartFollowPath()
    {
        if (_currentPath == null || _currentPath.Count <= 0)
        {
            List<AStarNode> newPath = _aStarAlgorithmManager.GetPath(this);

            SetCurrentPath(newPath);
        }
            
        ExecuteGridMove();
    }

    public void SetCurrentPath(List<AStarNode> currentPath)
    {
        _currentPath = currentPath;
    }

    /// <summary>
    /// 주어진 목표 월드 좌표로 이동하기 전에, 그리드 상에서 해당 칸을 점유(예약)하고,
    /// 이동이 가능하면 에이전트의 위치를 업데이트합니다.
    /// </summary>
    /// <param name="targetWorldCoordinate">이동하려는 목표 월드 좌표</param>
    private void ExecuteGridMove()
    {
        AStarNode currentNode = _currentPath[_currentPathIndex];
        Vector2Int destPos = new Vector2Int(currentNode.X, currentNode.Y);

        if (!IsAtEndOfPath && _grid.IsNodeBlocked(destPos))
        {
            ProcessOccupiedTileResponse(destPos);
            return;
        }

        AStarNode endNode = _currentPath[_currentPath.Count - 1];
        if (endNode.Agent == null)
        {
            RecalculatePath();
            return;
        }

        if (HasNextNode)
            MoveToNextNode(destPos);
        else if (IsAtEndOfPath)
            OnPathComplete();
    }

    private void MoveToNextNode(Vector2Int destPos)
    {
        UpdateAgentGridPosition(destPos);

        OnMove?.Invoke(destPos);
    }

    /// <summary>
    /// 현재 그리드 위치를 지정된 destPos로 업데이트하고, AStarGrid에도 해당 위치로 에이전트 정보를 갱신합니다.
    /// </summary>
    /// <param name="destPos">업데이트할 목표 월드 좌표</param>
    private void UpdateAgentGridPosition(Vector2Int destPos)
    {
        // 현재 그리드 위치를 업데이트
        CurrentGridPosition = destPos;

        // 그리드 시스템에 에이전트의 새로운 위치를 반영
        _grid.UpdateAgentGridPosition(this, destPos);
    }

    public void EndMove()
    {
        _currentPathIndex++;
    }

    private void SnapToLastValidPosition()
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

    private void ProcessOccupiedTileResponse(Vector2Int destPos)
    {
        TeamType crushAgentTeam = _grid.ReturnAgent(destPos).GetTeam();

        SnapToLastValidPosition();

        if (crushAgentTeam != GetTeam())
        {
            CrushOtherTeamAgent?.Invoke();
            Debug.Log($"적군 충돌 {gameObject.name} IdleState로 변경");
        }
        else
        {
            Debug.Log($"아군 충돌 {gameObject.name} 경로 재탐색");
            RecalculatePath();
        }
    }

    private void RecalculatePath()
    {
        ClearFllowing();
        StartFollowPath();
    }

    private void OnPathComplete()
    {
        OnPathCompleteAction?.Invoke();
    }

    public void ClearFllowing()
    {
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