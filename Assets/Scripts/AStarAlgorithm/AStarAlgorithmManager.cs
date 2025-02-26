using System.Collections.Generic;
using UnityEngine;

public class AStarAlgorithmManager : Singleton<AStarAlgorithmManager>
{
    private const int COST_STRAIGHT = 10;
    private const int COST_DIAGONAL = 14;

    private Vector2Int _bottomLeft;
    private Vector2Int _topRight;
    private List<AStarNode> _finalNodeList;
    private AStarNode[,] _nodeArray;
    private AStarNode _startNode, _targetNode, _currentNode;
    private List<AStarNode> _openList, _closedList;

    [SerializeField] private bool allowDiagonal;
    [SerializeField] private bool dontCrossCorner;

    protected override void Init()
    {
        base.Init();
    }

    public void CreateGridFromTilemap(Vector2Int topRight, Vector2Int bottomLeft)
    {
        _topRight = topRight;
        _bottomLeft = bottomLeft;

        int sizeX = _topRight.x - _bottomLeft.x + 1;
        int sizeY = _topRight.y - _bottomLeft.y + 1;
        _nodeArray = new AStarNode[sizeX, sizeY];

        int blockLayer = LayerMask.NameToLayer("Block");

        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                Vector2 tilePosition = new Vector2(i + _bottomLeft.x, j + _bottomLeft.y);
                bool isBlock = false;

                foreach (Collider2D col in Physics2D.OverlapCircleAll(tilePosition, 0.4f))
                {
                    if (col.gameObject.layer == blockLayer)
                    {
                        isBlock = true;
                        break;
                    }
                }

                _nodeArray[i, j] = new AStarNode(isBlock, i + _bottomLeft.x, j + _bottomLeft.y);
            }
        }
    }


    public void PathFinding(IAStarPathPoint startPoint, IAStarPathPoint targetPoint)
    {
        Vector2Int startVector = startPoint.PathPoint;
        Vector2Int targetVector = targetPoint.PathPoint;

        _startNode = _nodeArray[startVector.x - _bottomLeft.x, startVector.y - _bottomLeft.y];
        _targetNode = _nodeArray[targetVector.x - _bottomLeft.x, targetVector.y - _bottomLeft.y];

        _openList = new List<AStarNode> { _startNode };
        _closedList = new List<AStarNode>();
        _finalNodeList = new List<AStarNode>();

        while (_openList.Count > 0)
        {
            // _openList에서 F값이 가장 낮은 노드를 선택 (F가 같으면 H값이 낮은 것을 우선)
            _currentNode = _openList[0];
            for (int i = 1; i < _openList.Count; i++)
            {
                AStarNode node = _openList[i];
                if (node.F < _currentNode.F || (node.F == _currentNode.F && node.H < _currentNode.H))
                {
                    _currentNode = node;
                }
            }

            _openList.Remove(_currentNode);
            _closedList.Add(_currentNode);

            // 목표 노드에 도달한 경우 경로를 구성하고 종료
            if (_currentNode == _targetNode)
            {
                BuildFinalPath();
                return;
            }

            EvaluateNeighbors(_currentNode);
        }
    }

    private void EvaluateNeighbors(AStarNode node)
    {
        if (allowDiagonal)
        {
            OpenListAdd(node.X + 1, node.Y + 1);
            OpenListAdd(node.X - 1, node.Y + 1);
            OpenListAdd(node.X - 1, node.Y - 1);
            OpenListAdd(node.X + 1, node.Y - 1);
        }

        OpenListAdd(node.X, node.Y + 1);
        OpenListAdd(node.X + 1, node.Y);
        OpenListAdd(node.X, node.Y - 1);
        OpenListAdd(node.X - 1, node.Y);
    }

    private void OpenListAdd(int checkX, int checkY)
    {
        // 좌표가 그리드 내에 있는지 확인
        if (checkX < _bottomLeft.x || checkX > _topRight.x || checkY < _bottomLeft.y || checkY > _topRight.y)
        {
            return;
        }

        AStarNode neighborNode = _nodeArray[checkX - _bottomLeft.x, checkY - _bottomLeft.y];

        // 이웃 노드가 블록이거나 이미 검사한 노드면 리턴
        if (neighborNode.IsBlock || _closedList.Contains(neighborNode))
        {
            return;
        }

        // 대각 이동 시 코너 크로싱 제한 체크
        if (allowDiagonal)
        {
            if (_nodeArray[_currentNode.X - _bottomLeft.x, checkY - _bottomLeft.y].IsBlock &&
                _nodeArray[checkX - _bottomLeft.x, _currentNode.Y - _bottomLeft.y].IsBlock)
            {
                return;
            }
        }

        // 코너 크로싱 금지 옵션 체크 (대각 이동 여부와 관계없이)
        if (dontCrossCorner)
        {
            if (_nodeArray[_currentNode.X - _bottomLeft.x, checkY - _bottomLeft.y].IsBlock ||
                _nodeArray[checkX - _bottomLeft.x, _currentNode.Y - _bottomLeft.y].IsBlock)
            {
                return;
            }
        }

        // 이동 비용 계산 (상하좌우: 10, 대각선: 14)
        int moveCost = _currentNode.G + ((_currentNode.X - checkX == 0 || _currentNode.Y - checkY == 0) ? COST_STRAIGHT : COST_DIAGONAL);

        if (moveCost < neighborNode.G || !_openList.Contains(neighborNode))
        {
            neighborNode.G = moveCost;
            neighborNode.H = (Mathf.Abs(neighborNode.X - _targetNode.X) + Mathf.Abs(neighborNode.Y - _targetNode.Y)) * COST_STRAIGHT;
            neighborNode.ParentNode = _currentNode;

            if (!_openList.Contains(neighborNode))
            {
                _openList.Add(neighborNode);
            }
        }
    }

    private void BuildFinalPath()
    {
        AStarNode node = _targetNode;
        while (node != _startNode)
        {
            _finalNodeList.Add(node);
            node = node.ParentNode;
        }
        _finalNodeList.Add(_startNode);
        _finalNodeList.Reverse();

        for (int i = 0; i < _finalNodeList.Count; i++)
        {
            Debug.Log($"{i}번째는 {_finalNodeList[i].X}, {_finalNodeList[i].Y}");
        }
    }

    private bool IsDrawLine => _finalNodeList != null && _finalNodeList.Count > 0;

    private void OnDrawGizmos()
    {
        if (IsDrawLine)
        {
            for (int i = 0; i < _finalNodeList.Count - 1; i++)
            {
                Vector2 from = new Vector2(_finalNodeList[i].X, _finalNodeList[i].Y);
                Vector2 to = new Vector2(_finalNodeList[i + 1].X, _finalNodeList[i + 1].Y);
                Gizmos.DrawLine(from, to);
            }
        }
    }
}
