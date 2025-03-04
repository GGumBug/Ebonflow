using UnityEngine;

public class AStarGrid
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

    public AStarGrid(IAStarGridSettings gridSettings)
    {
        _gridBottomLeft = gridSettings.GridBottomLeft;
        _gridTopRight = gridSettings.GridTopRight;

        int sizeX = _gridTopRight.x - _gridBottomLeft.x + 1;
        int sizeY = _gridTopRight.y - _gridBottomLeft.y + 1;

        _grid = new AStarNode[sizeX, sizeY];

        CreateGridFromTilemap(sizeX, sizeY);
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

    public void SetNodeBlock(Vector2Int worldCoordinate, bool isBlock)
    {
        Vector2Int gridIndex = WorldToGridIndex(worldCoordinate);

        if (IsOutOfBounds(worldCoordinate))
            Debug.LogError("SetNodeBlock: 주어진 좌표가 그리드 범위를 벗어났습니다.");
        else
            _grid[gridIndex.x, gridIndex.y].IsBlock = isBlock;
    }

    /// <summary>
    /// 지정된 'fromWorldCoordinate' 위치에 있는 블록(isBlock)이 'toWorldCoordinate'로 이동하도록 업데이트합니다.
    /// </summary>
    /// <param name="fromWorldCoordinate">블록이 현재 위치한 월드 좌표</param>
    /// <param name="toWorldCoordinate">블록이 이동할 목표 월드 좌표</param>
    public void UpdateAgentGridPosition(Vector2Int fromWorldCoordinate, Vector2Int toWorldCoordinate)
    {
        Vector2Int toGridIndex = WorldToGridIndex(toWorldCoordinate);

        if (IsOutOfBounds(toWorldCoordinate))
        {
            Debug.LogError("MoveBlock: 대상 좌표가 그리드 범위를 벗어났습니다.");
            return;
        }

        if (_grid[toGridIndex.x, toGridIndex.y].IsBlock)
        {
            Debug.LogWarning("MoveBlock: 대상 셀이 이미 Block 상태입니다.");
            return;
        }

        SetNodeBlock(fromWorldCoordinate, false);

        SetNodeBlock(toWorldCoordinate, true);
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
        SetNodeBlock(startWorldCoordinate, lockEndpoints);
        SetNodeBlock(endWorldCoordinate, lockEndpoints);
    }
}
