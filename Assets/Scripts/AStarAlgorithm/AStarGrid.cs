using AutoBattle;
using AutoBattle.Input;
using System;
using UnityEngine;

public class AStarGrid : MonoBehaviour, IGridManager
{
    private const float TILE_COLLIDER_RADIUS = 0.4f;

    [Header("Placement")]
    [SerializeField, Min(0)]
    private int placeableRows = 3; // 0..(placeableRows-1) 행만 배치 가능

    private Vector2Int _gridBottomLeft;
    private Vector2Int _gridTopRight;
    private AStarNode[,] _grid;
    private IBattleRoster _roster;

    // 물리 쿼리 성능 최적화(NonAlloc)
    private int _blockMask;
    private readonly Collider2D[] _hitBuffer = new Collider2D[4];

    private Func<bool> requestCanDrop;

    public GridType Type => GridType.Battle;
    public bool CanDrop => requestCanDrop?.Invoke() ?? false;

    public bool IsOutOfBounds(int x, int y) =>
        _grid == null ||
        x < 0 || x >= _grid.GetLength(0) ||
        y < 0 || y >= _grid.GetLength(1);

    public bool IsOutOfBounds(Vector2Int toGridIndex) =>
        _grid == null ||
        toGridIndex.x < 0 || toGridIndex.x >= _grid.GetLength(0) ||
        toGridIndex.y < 0 || toGridIndex.y >= _grid.GetLength(1);

    public void Init(IAStarGridSettings gridSettings)
    {
        _gridBottomLeft = gridSettings.GridBottomLeft;
        _gridTopRight = gridSettings.GridTopRight;

        int sizeX = _gridTopRight.x - _gridBottomLeft.x + 1;
        int sizeY = _gridTopRight.y - _gridBottomLeft.y + 1;

        _grid = new AStarNode[sizeX, sizeY];

        _blockMask = 1 << Constants.BLOCK_LAYER;
        CreateGridFromTilemap(sizeX, sizeY);

        requestCanDrop = () =>
            AutoBattleManager.Instance != null &&
            AutoBattleManager.Instance.StateController.GameState == AutoBattleGameState.PreparationPhase;
    }

    public void RegisterBattleRoster(IBattleRoster battleRoster)
    {
        _roster = battleRoster;
    }

    private bool IsNodePlaceable(Vector2Int worldCoordinate)
    {
        Vector2Int gridIndex = WorldToGridIndex(worldCoordinate);
        if (IsOutOfBounds(gridIndex))
            return false;

        return _grid[gridIndex.x, gridIndex.y].IsPlaceable;
    }

    /// <summary>주어진 월드 좌표의 노드가 Block인지 확인</summary>
    public bool IsNodeBlocked(Vector2Int worldCoordinate)
    {
        Vector2Int gridIndex = WorldToGridIndex(worldCoordinate);

        if (IsOutOfBounds(gridIndex))
        {
            Debug.LogError("IsNodeBlocked: 주어진 좌표가 그리드 범위를 벗어났습니다.");
            return false;
        }

        return _grid[gridIndex.x, gridIndex.y].GetBlock;
    }

    public AStarAgent ReturnAgent(Vector2Int worldCoordinate)
    {
        Vector2Int gridIndex = WorldToGridIndex(worldCoordinate);

        if (IsOutOfBounds(gridIndex))
        {
            Debug.LogError("ReturnAgent: 주어진 좌표가 그리드 범위를 벗어났습니다.");
            return null;
        }

        return _grid[gridIndex.x, gridIndex.y].Agent;
    }

    private Vector2Int WorldToGridIndex(Vector2Int worldCoordinate)
    {
        int xIndex = worldCoordinate.x - _gridBottomLeft.x;
        int yIndex = worldCoordinate.y - _gridBottomLeft.y;
        return new Vector2Int(xIndex, yIndex);
    }

    private void CreateGridFromTilemap(int sizeX, int sizeY)
    {
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                int x = i + _gridBottomLeft.x;
                int y = j + _gridBottomLeft.y;
                Vector2 tilePosition = new Vector2(x, y);

                var hit = Physics2D.OverlapCircle(tilePosition, TILE_COLLIDER_RADIUS, _blockMask);
                bool isBlock = hit != null;

                bool isPlaceable = j < placeableRows;

                _grid[i, j] = new AStarNode(isBlock, isPlaceable, x, y);
            }
        }
    }


    public AStarNode GetNodeAt(int x, int y)
    {
        Vector2Int gridIndex = WorldToGridIndex(new Vector2Int(x, y));

        if (IsOutOfBounds(gridIndex))
            throw new Exception("GetNodeAt: 주어진 좌표가 그리드 범위를 벗어났습니다.");

        return _grid[gridIndex.x, gridIndex.y];
    }

    /// <summary>월드 좌표 기준 Block/Agent 설정</summary>
    public void SetNodeBlock(Vector2Int worldCoordinate, bool isBlock, AStarAgent agent = null)
    {
        Vector2Int gridIndex = WorldToGridIndex(worldCoordinate);

        if (IsOutOfBounds(gridIndex))
        {
            Debug.LogError("SetNodeBlock: 주어진 좌표가 그리드 범위를 벗어났습니다.");
            return;
        }

        var targetNode = _grid[gridIndex.x, gridIndex.y];
        targetNode.SetBlock = isBlock;
        targetNode.Agent = agent;
    }

    public void RemoveNodeBlock(Vector2Int worldCoordinate)
    {
        Vector2Int gridIndex = WorldToGridIndex(worldCoordinate);

        if (IsOutOfBounds(gridIndex))
        {
            Debug.LogError("RemoveNodeBlock: 주어진 좌표가 그리드 범위를 벗어났습니다.");
            return;
        }

        var targetNode = _grid[gridIndex.x, gridIndex.y];
        targetNode.SetBlock = false;
        targetNode.Agent = null;
    }

    /// <summary>
    /// 지정된 에이전트를 from(현재) → to(목표) 월드 좌표로 이동시켜 그리드 상태를 갱신합니다.
    /// </summary>
    public void UpdateAgentGridPosition(AStarAgent agent, Vector2Int toWorldCoordinate)
    {
        Vector2Int toGridIndex = WorldToGridIndex(toWorldCoordinate);

        if (IsOutOfBounds(toGridIndex))
        {
            Debug.LogError("MoveBlock: 대상 좌표가 그리드 범위를 벗어났습니다.");
            return;
        }

        if (_grid[toGridIndex.x, toGridIndex.y].GetBlock)
        {
            Debug.LogWarning("MoveBlock: 대상 셀이 이미 Block 상태입니다.");
            return;
        }

        // 월드 좌표 기준 API 일관 사용
        SetNodeBlock(agent.PathPoint, false);
        SetNodeBlock(toWorldCoordinate, true, agent);
    }

    /// <summary>
    /// 경로 탐색 전/후 엔드포인트를 잠그거나 해제합니다.
    /// </summary>
    public void SetPathEndpointsLockState(bool lockEndpoints, Vector2Int startWorldCoordinate, Vector2Int endWorldCoordinate)
    {
        AStarNode startNode = GetNodeAt(startWorldCoordinate.x, startWorldCoordinate.y);
        startNode.SetBlock = lockEndpoints;

        AStarNode targetNode = GetNodeAt(endWorldCoordinate.x, endWorldCoordinate.y);
        targetNode.SetBlock = lockEndpoints;
    }

    public bool IsValidCell(Vector2Int cell) => !IsOutOfBounds(cell);

    public bool IsCellOccupied(Vector2Int cell)
    {
        if (IsOutOfBounds(cell)) return true;
        if (!IsNodePlaceable(cell)) return true;
        return IsNodeBlocked(cell);
    }

    public void PlaceUnit(IUnitDraggable draggable, Vector2Int cell)
    {
        var agent = draggable.Unit.Agent;
        if (agent == null)
        {
            Debug.LogError("PlaceUnit: AStarAgent 컴포넌트를 찾을 수 없습니다.");
            return;
        }

        Vector3 originPos = draggable.OriginalPosition;
        Vector2Int originPosInt = new Vector2Int(Mathf.RoundToInt(originPos.x), Mathf.RoundToInt(originPos.y));

        if (!IsOutOfBounds(originPosInt))
            SetNodeBlock(originPosInt, false);

        SetNodeBlock(cell, true, agent);
        draggable.Unit.SetSnapTransform(cell);
        draggable.Unit.RegisterPlacement(Type);

        SyncRosterOnPlace(draggable.Unit);
    }

    public void RemoveUnit(Vector2Int pos, Unit unit)
    {
        if (!IsOutOfBounds(pos))
            SetNodeBlock(pos, false);

        SyncRosterOnRemove(unit);
    }

    public void SyncRosterOnPlace(Unit unit)
    {
        _roster ??= AutoBattleUnitManager.Instance?.Roster;

        if (_roster != null && !_roster.Contains(unit))
            _roster.Register(unit);
    }

    private void SyncRosterOnRemove(Unit unit)
    {
        if (_roster != null && _roster.Contains(unit))
            _roster.Unregister(unit);
    }

    private void OnDrawGizmos()
    {
        if (_grid == null)
            return;

        for (int i = 0; i < _grid.GetLength(0); i++)
        {
            for (int j = 0; j < _grid.GetLength(1); j++)
            {
                AStarNode node = _grid[i, j];
                Vector2 pos = new Vector2(node.X, node.Y);

                if (node.GetBlock && node.Agent != null)
                    Gizmos.color = Color.red;
                else if (node.GetBlock)
                    Gizmos.color = Color.yellow;
                else if (!node.IsPlaceable)
                    Gizmos.color = Color.cyan;
                else
                    Gizmos.color = Color.green;

                Gizmos.DrawWireSphere(pos, TILE_COLLIDER_RADIUS);
            }
        }
    }
}
