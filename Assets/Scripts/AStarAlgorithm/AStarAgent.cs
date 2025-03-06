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

    [field: SerializeField] public TeamType Team { get; private set; }

    private List<AStarNode> _currentPath;
    private int _currentPathIndex = 1;
    private AStarGrid _grid;

    public Vector2Int CurrentGridPosition { get; private set; }
    public event Func<AStarAgent, bool, bool, List<AStarNode>> OnTargetLost;

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
        transform.position = (Vector3Int)PathPoint;
        CurrentGridPosition = PathPoint;
    }

    public void MarkCurrentPositionAsBlocked()
    {
        _grid ??= AStarAlgorithmManager.Instance.Grid;
        _grid.SetNodeBlock(PathPoint, true, this);
    }

    public void SetCurrentPath(List<AStarNode> currentPath)
    {
        _currentPath = currentPath;
    }

    public void BeginPathFollowing()
    {
        if (_currentPath != null)
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

        CheckTargetOccupancyAndRecalculate(destPos);

        if (!IsAtEndOfPath && _grid.IsNodeBlocked(destPos))
        {
            //로직 업데이트 필요
            ProcessOccupiedTileResponse(destPos);
            return;
        }

        if (HasNextNode)
            MoveToNextNode(destPos);
        else if (IsAtEndOfPath)
            StopMovement();
    }

    /// <summary>
    /// 지정된 목적지 좌표에 해당하는 노드가 점유되어 있지 않으면 경로를 재탐색합니다.
    /// </summary>
    /// <param name="destPos">목적지 월드 좌표</param>
    private void CheckTargetOccupancyAndRecalculate(Vector2Int destPos)
    {
        AStarNode targetNode = _grid.GetNodeAt(destPos.x, destPos.y);
        if (targetNode.Agent == null)
            OnTargetLost.Invoke(this, true, true);
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

        // DOTween을 사용하여 선형 보간으로 이동시키고, 이동이 완료되면 SnapToDestinationAndAdvance를 호출합니다.
        transform.DOMove(destination, duration)
                 .SetEase(Ease.Linear)
                 .SetDelay(stepDelay)
                 .OnComplete(() => SnapAndAdvance(destPos));
    }

    private void SnapAndAdvance(Vector2Int destPos)
    {
        transform.position = new Vector2(destPos.x, destPos.y);
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
        TeamType crushAgentTeam = _grid.ReturnAgent(destPos).Team;

        if (crushAgentTeam != Team)
        {
            SnapToLastValidPosition();
            StopMovement();
            Debug.Log("공격 스테이트로 전환");
        }
        else
        {
            SnapToLastValidPosition();
            RecalculatePath();
        }
    }

    private void RecalculatePath()
    {
        ClearFllowing();
        BeginPathFollowing();
    }

    private void StopMovement()
    {
        ClearFllowing();
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
