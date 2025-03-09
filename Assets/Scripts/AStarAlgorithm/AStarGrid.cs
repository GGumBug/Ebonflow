using UnityEngine;

public class AStarGrid : MonoBehaviour
{
    private const float TILE_COLLIDER_RADIUS = 0.4f;

    private Vector2Int _gridBottomLeft;
    private Vector2Int _gridTopRight;
    private AStarNode[,] _grid;

    public bool IsOutOfBounds(int x, int y) => 
        x < 0 || x >= _grid.GetLength(0) || 
        y < 0 || y >= _grid.GetLength(1);

    public bool IsOutOfBounds(Vector2Int toGridIndex) => 
        toGridIndex.x < 0 || toGridIndex.x >= _grid.GetLength(0) || 
        toGridIndex.y < 0 || toGridIndex.y >= _grid.GetLength(1);

    public void Init(IAStarGridSettings gridSettings)
    {
        _gridBottomLeft = gridSettings.GridBottomLeft;
        _gridTopRight = gridSettings.GridTopRight;

        int sizeX = _gridTopRight.x - _gridBottomLeft.x + 1;
        int sizeY = _gridTopRight.y - _gridBottomLeft.y + 1;

        _grid = new AStarNode[sizeX, sizeY];

        CreateGridFromTilemap(sizeX, sizeY);
    }

    /// <summary>
    /// 주어진 월드 좌표에 해당하는 AStarNode가 Block 상태인지 확인합니다.
    /// </summary>
    /// <param name="worldCoordinate">확인할 월드 좌표</param>
    /// <returns>해당 노드가 Block이면 true, 아니면 false</returns>
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
            Debug.LogError("IsNodeBlocked: 주어진 좌표가 그리드 범위를 벗어났습니다.");
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
        int mask = (1 << Constants.BLOCK_LAYER);

        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                Vector2 tilePosition = new Vector2(i + _gridBottomLeft.x, j + _gridBottomLeft.y);

                Collider2D[] colliders = Physics2D.OverlapCircleAll(tilePosition, TILE_COLLIDER_RADIUS, mask);
                bool isBlock = colliders.Length > 0;

                _grid[i, j] = new AStarNode(isBlock, i + _gridBottomLeft.x, j + _gridBottomLeft.y);
            }
        }
    }

    public AStarNode GetNodeAt(int x, int y)
    {
        Vector2Int gridIndex = WorldToGridIndex(new Vector2Int(x, y));

        if (IsOutOfBounds(gridIndex))
            throw new System.Exception("SetNodeBlock: 주어진 좌표가 그리드 범위를 벗어났습니다.");
        else
            return _grid[gridIndex.x, gridIndex.y];
    }

    public void SetNodeBlock(Vector2Int worldCoordinate, bool isBlock, AStarAgent agent = null)
    {
        Vector2Int gridIndex = WorldToGridIndex(worldCoordinate);

        if (IsOutOfBounds(worldCoordinate))
        {
            Debug.LogError("SetNodeBlock: 주어진 좌표가 그리드 범위를 벗어났습니다.");
        }
        else
        {
            var targetNode = _grid[gridIndex.x, gridIndex.y];
            targetNode.SetBlock = isBlock;
            targetNode.Agent = agent;
        }
    }

    /// <summary>
    /// 지정된 'fromWorldCoordinate' 위치에 있는 블록(isBlock)이 'toWorldCoordinate'로 이동하도록 업데이트합니다.
    /// </summary>
    /// <param name="fromWorldCoordinate">블록이 현재 위치한 월드 좌표</param>
    /// <param name="toWorldCoordinate">블록이 이동할 목표 월드 좌표</param>
    public void UpdateAgentGridPosition(AStarAgent agent, Vector2Int toWorldCoordinate)
    {
        Vector2Int fromGridIndex = WorldToGridIndex(agent.PathPoint);
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

        SetNodeBlock(fromGridIndex, false);

        SetNodeBlock(toGridIndex, true, agent);
    }

    /// <summary>
    /// 주어진 시작 및 종료 월드 좌표에 해당하는 노드의 Block 상태를, 
    /// lockEndpoints 값에 따라 설정합니다.
    /// 경로 탐색 전에는 false (Block 해제), 탐색 완료 후에는 true (Block 설정)으로 사용할 수 있습니다.
    /// </summary>
    /// <param name="lockEndpoints">true이면 해당 노드를 Block 상태로 설정하고, false이면 Block 상태를 해제합니다.</param>
    /// <param name="startWorldCoordinate">시작 위치의 월드 좌표</param>
    /// <param name="endWorldCoordinate">종료 위치의 월드 좌표</param>
    public void SetPathEndpointsLockState(bool lockEndpoints, Vector2Int startWorldCoordinate, Vector2Int endWorldCoordinate)
    {
        AStarNode startNode = GetNodeAt(startWorldCoordinate.x, startWorldCoordinate.y);
        startNode.SetBlock = lockEndpoints;

        AStarNode targetNode = GetNodeAt(endWorldCoordinate.x, endWorldCoordinate.y);
        targetNode.SetBlock = lockEndpoints;
    }

    private void OnDrawGizmos()
    {
        if (_grid == null)
            return;

        // _grid 배열의 모든 셀을 순회
        for (int i = 0; i < _grid.GetLength(0); i++)
        {
            for (int j = 0; j < _grid.GetLength(1); j++)
            {
                AStarNode node = _grid[i, j];
                // 노드의 좌표를 Vector2로 변환 (노드에 저장된 좌표는 world 좌표여야 합니다)
                Vector2 pos = new Vector2(node.X, node.Y);
                // isBlock 상태에 따라 색상을 설정
                if (node.GetBlock && node.Agent)
                    Gizmos.color = Color.red;
                else if (node.GetBlock)
                    Gizmos.color = Color.yellow;
                else
                    Gizmos.color = Color.green;

                // 0.4f 반지름의 원을 그립니다.
                Gizmos.DrawWireSphere(pos, TILE_COLLIDER_RADIUS);
            }
        }
    }
}
