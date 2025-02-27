using UnityEngine;
using System.Collections.Generic;

public class AStarAlgorithmManager : Singleton<AStarAlgorithmManager>
{
    private const int COST_STRAIGHT = 10;
    private const int COST_DIAGONAL = 14;

    private bool _allowDiagonal, _dontCrossCorner;
    private Vector2Int _gridBottomLeft;
    private Vector2Int _gridTopRight;
    private AStarNode[,] _gridNodes;
    private AStarNode _startNode, _targetNode, _currentNode;
    private HashSet<AStarNode> _closedSet;
    private PriorityQueue<AStarNode> _openNodeQueue;
    private List<AStarNode> _finalPathNodes;

    protected override void Init()
    {
        base.Init();
    }

    public void CreateGridFromTilemap(Vector2Int gridTopRight, Vector2Int gridBottomLeft)
    {
        _gridTopRight = gridTopRight;
        _gridBottomLeft = gridBottomLeft;

        int sizeX = _gridTopRight.x - _gridBottomLeft.x + 1;
        int sizeY = _gridTopRight.y - _gridBottomLeft.y + 1;
        _gridNodes = new AStarNode[sizeX, sizeY];

        int blockLayer = LayerMask.NameToLayer("Block");

        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                Vector2 tilePosition = new Vector2(i + _gridBottomLeft.x, j + _gridBottomLeft.y);
                bool isBlock = false;

                foreach (Collider2D col in Physics2D.OverlapCircleAll(tilePosition, 0.4f))
                {
                    if (col.gameObject.layer == blockLayer)
                    {
                        isBlock = true;
                        break;
                    }
                }

                _gridNodes[i, j] = new AStarNode(isBlock, i + _gridBottomLeft.x, j + _gridBottomLeft.y);
            }
        }
    }

    public void FindPath(IAStarPathPoint startPoint, IAStarPathPoint targetPoint, bool allowDiagonal = false, bool dontCrossCorner = false)
    {
        Vector2Int startVector = startPoint.PathPoint;
        Vector2Int targetVector = targetPoint.PathPoint;

        _allowDiagonal = allowDiagonal;
        _dontCrossCorner = dontCrossCorner;

        _startNode = GetNodeAt(startVector.x, startVector.y);
        _targetNode = GetNodeAt(targetVector.x, targetVector.y);

        _openNodeQueue = new PriorityQueue<AStarNode>(5, SortOrder.Ascending);
        _closedSet = new HashSet<AStarNode>();
        _finalPathNodes = new List<AStarNode>();

        _openNodeQueue.Enqueue(_startNode, _startNode.F);

        while (_openNodeQueue.Count > 0)
        {
            _currentNode = _openNodeQueue.Dequeue();
            _closedSet.Add(_currentNode);

            if (_currentNode == _targetNode)
            {
                ConstructFinalPath();
                return;
            }

            EvaluateAdjacentNodes(_currentNode);
        }
    }

    private void EvaluateAdjacentNodes(AStarNode node)
    {
        // 대각선 이동 가능 여부에 따라 처리
        if (_allowDiagonal)
        {
            foreach (var dir in Constants.DIAGONAL_DIRECTIONS)
            {
                TryAddToOpenQueue(node.X + dir.x, node.Y + dir.y);
            }
        }
        foreach (var dir in Constants.ORTHOGONAL_DIRECTIONS)
        {
            TryAddToOpenQueue(node.X + dir.x, node.Y + dir.y);
        }
    }

    private void TryAddToOpenQueue(int checkX, int checkY)
    {
        // 그리드 범위 내에 있는지 검사
        if (!IsWithinGridBounds(checkX, checkY))
            return;

        AStarNode neighborNode = GetNodeAt(checkX, checkY);

        // 블록이거나 이미 처리한 노드면 건너뜁니다.
        if (neighborNode.IsBlock || _closedSet.Contains(neighborNode))
            return;

        // 대각선 이동 시, 코너 크로싱 제한 검사
        if (_allowDiagonal)
        {
            AStarNode adjacent1 = GetNodeAt(_currentNode.X, checkY);
            AStarNode adjacent2 = GetNodeAt(checkX, _currentNode.Y);
            if (adjacent1.IsBlock && adjacent2.IsBlock)
                return;
        }

        // 코너 크로싱 금지 옵션 검사
        if (_dontCrossCorner)
        {
            AStarNode adjacent1 = GetNodeAt(_currentNode.X, checkY);
            AStarNode adjacent2 = GetNodeAt(checkX, _currentNode.Y);
            if (adjacent1.IsBlock || adjacent2.IsBlock)
                return;
        }

        int moveCost = _currentNode.G + CalculateMoveCost(_currentNode.X, _currentNode.Y, checkX, checkY);

        if (moveCost < neighborNode.G || !_openNodeQueue.Contains(neighborNode))
        {
            neighborNode.G = moveCost;
            neighborNode.H = (Mathf.Abs(neighborNode.X - _targetNode.X) + Mathf.Abs(neighborNode.Y - _targetNode.Y)) * COST_STRAIGHT;
            neighborNode.ParentNode = _currentNode;

            if (!_openNodeQueue.Contains(neighborNode))
                _openNodeQueue.Enqueue(neighborNode, neighborNode.F);
        }
    }

    // 그리드 범위 체크를 위한 헬퍼 메서드
    private bool IsWithinGridBounds(int x, int y)
    {
        return x >= _gridBottomLeft.x && x <= _gridTopRight.x && y >= _gridBottomLeft.y && y <= _gridTopRight.y;
    }

    // 주어진 그리드 좌표에 해당하는 노드를 반환
    private AStarNode GetNodeAt(int x, int y)
    {
        return _gridNodes[x - _gridBottomLeft.x, y - _gridBottomLeft.y];
    }

    // 이동 비용 계산 (상하좌우와 대각선 이동 비용 차이를 적용)
    private int CalculateMoveCost(int fromX, int fromY, int toX, int toY)
    {
        return (fromX == toX || fromY == toY) ? COST_STRAIGHT : COST_DIAGONAL;
    }

    private void ConstructFinalPath()
    {
        AStarNode node = _targetNode;
        while (node != _startNode)
        {
            _finalPathNodes.Add(node);
            node = node.ParentNode;
        }
        _finalPathNodes.Add(_startNode);
        _finalPathNodes.Reverse();

        for (int i = 0; i < _finalPathNodes.Count; i++)
        {
            Debug.Log($"{i}번째는 {_finalPathNodes[i].X}, {_finalPathNodes[i].Y}");
        }
    }

    private bool IsDrawLine => _finalPathNodes != null && _finalPathNodes.Count > 0;

    private void OnDrawGizmos()
    {
        if (IsDrawLine)
        {
            for (int i = 0; i < _finalPathNodes.Count - 1; i++)
            {
                Vector2 from = new Vector2(_finalPathNodes[i].X, _finalPathNodes[i].Y);
                Vector2 to = new Vector2(_finalPathNodes[i + 1].X, _finalPathNodes[i + 1].Y);
                Gizmos.DrawLine(from, to);
            }
        }
    }
}
