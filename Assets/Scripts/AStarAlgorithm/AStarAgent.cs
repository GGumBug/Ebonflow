using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AStarAgent : MonoBehaviour, IAStarPathPoint, IAStarPathFollower
{
    [Header("Movement Settings")]
    [Tooltip("초당 이동 속도 (단위: 유닛)")]
    [SerializeField] private float moveSpeed = 6f;
    [Tooltip("각 타일로 이동할 때 적용되는 지연 시간 (초 단위)")]
    [SerializeField] private float stepDelay = 0.5f;

    [Header("Debug Settings")]
    [Tooltip("경로를 Gizmos로 그릴지 여부 (디버깅용)")]
    [SerializeField] private bool isDrawLine;

    private int _currentPathIndex = 1;
    private List<AStarNode> _currentPath;
    private AStarAlgorithmManager _aStarAlgorithmManager;
    private AStarGrid _grid;
    private Tween _moveTween;

    public Vector2Int CurrentGridPosition { get; private set; }
    public event Action OnBeginWalk;
    public event Func<TeamType> OnRequestTeamType;
    public event Func<bool> OnAttackInitiated;

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

    public void StartFollowPath()
    {
        List<AStarNode> newPath = _aStarAlgorithmManager.GetPath(this);

        SetCurrentPath(newPath);
        BeginPathFollowing();
    }

    public void SetCurrentPath(List<AStarNode> currentPath)
    {
        _currentPath = currentPath;
    }

    public void BeginPathFollowing()
    {
        if (_currentPath != null && _currentPath.Count > 0 )
            ExecuteGridMove();
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

        RecalculatePathIfTargetMissing();

        if (!IsAtEndOfPath && _grid.IsNodeBlocked(destPos))
        {
            ProcessOccupiedTileResponse(destPos);
            return;
        }

        if (HasNextNode)
            MoveToNextNode(destPos);
        else if (IsAtEndOfPath)
            StopMovement();
    }

    private void RecalculatePathIfTargetMissing()
    {
        AStarNode endNode = _currentPath[_currentPath.Count - 1];
        if (endNode.Agent == null)
            StartFollowPath();
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

    private void MoveToNextNode(Vector2Int destPos)
    {
        UpdateAgentGridPosition(destPos);

        // 목표 위치를 Vector3로 변환 (z값은 현재 위치 유지)
        Vector3 destination = new Vector3(destPos.x, destPos.y, transform.position.z);

        // 현재 위치와 목표 위치 사이의 거리를 계산하고, 이동 시간(duration)을 결정합니다.
        float distance = Vector2.Distance(transform.position, new Vector2(destPos.x, destPos.y));
        float duration = distance / moveSpeed;

        OnBeginWalk.Invoke();

        // DOTween을 사용하여 선형 보간으로 이동시키고, 이동이 완료되면 SnapAndAdvance를 호출합니다.
        _moveTween = transform.DOMove(destination, duration)
                    .SetEase(Ease.Linear)
                    .SetDelay(stepDelay)
                    .OnComplete(() => SnapAndAdvance(destPos));
    }

    private void SnapAndAdvance(Vector2Int destPos)
    {
        transform.position = new Vector2(destPos.x, destPos.y);

        if (OnAttackInitiated.Invoke())
        {
            StopMovement();
            return;
        }

        _currentPathIndex++;
        ExecuteGridMove();
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
            StopMovement();
            OnAttackInitiated?.Invoke();
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

    public void StopMovement()
    {
        ClearFllowing();

        if (_moveTween != null)
            _moveTween.Kill();
    }

    private void ClearFllowing()
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
