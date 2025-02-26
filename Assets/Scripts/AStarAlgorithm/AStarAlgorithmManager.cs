using System.Collections.Generic;
using UnityEngine;

public class AStarAlgorithmManager : Singleton<AStarAlgorithmManager>
{
    public Vector2Int bottomLeft, topRight, startPos, targetPos;
    private List<AStarNode> _finalNodeList;
    public bool allowDiagonal, dontCrossCorner;

    int sizeX, sizeY;
    AStarNode[,] NodeArray;
    AStarNode StartNode, TargetNode, CurNode;
    List<AStarNode> OpenList, ClosedList;

    public void PathFinding()
    {
        sizeX = topRight.x - bottomLeft.x + 1;
        sizeY = topRight.y - bottomLeft.y + 1;
        NodeArray = new AStarNode[sizeX, sizeY];

        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                bool isWall = false;
                foreach (Collider2D col in Physics2D.OverlapCircleAll(new Vector2(i + bottomLeft.x, j + bottomLeft.y), 0.4f))
                    if (col.gameObject.layer == LayerMask.NameToLayer("Wall")) isWall = true;

                NodeArray[i, j] = new AStarNode(isWall, i + bottomLeft.x, j + bottomLeft.y);
            }
        }

        StartNode = NodeArray[startPos.x - bottomLeft.x, startPos.y - bottomLeft.y];
        TargetNode = NodeArray[targetPos.x - bottomLeft.x, targetPos.y - bottomLeft.y];

        OpenList = new List<AStarNode>() { StartNode };
        ClosedList = new List<AStarNode>();
        _finalNodeList = new List<AStarNode>();

        while (OpenList.Count > 0)
        {
            CurNode = OpenList[0];
            for (int i = 1; i < OpenList.Count; i++)
                if (OpenList[i].F <= CurNode.F && OpenList[i].H < CurNode.H) CurNode = OpenList[i];

            OpenList.Remove(CurNode);
            ClosedList.Add(CurNode);

            if (CurNode == TargetNode)
            {
                AStarNode TargetCurNode = TargetNode;
                while (TargetCurNode != StartNode)
                {
                    _finalNodeList.Add(TargetCurNode);
                    TargetCurNode = TargetCurNode.ParentNode;
                }
                _finalNodeList.Add(StartNode);
                _finalNodeList.Reverse();

                for (int i = 0; i < _finalNodeList.Count; i++)
                    print(i + "번째는 " + _finalNodeList[i].X + ", " + _finalNodeList[i].Y);
                return;
            }

            if (allowDiagonal)
            {
                OpenListAdd(CurNode.X + 1, CurNode.Y + 1);
                OpenListAdd(CurNode.X - 1, CurNode.Y + 1);
                OpenListAdd(CurNode.X - 1, CurNode.Y - 1);
                OpenListAdd(CurNode.X + 1, CurNode.Y - 1);
            }

            OpenListAdd(CurNode.X, CurNode.Y + 1);
            OpenListAdd(CurNode.X + 1, CurNode.Y);
            OpenListAdd(CurNode.X, CurNode.Y - 1);
            OpenListAdd(CurNode.X - 1, CurNode.Y);
        }
    }

    void OpenListAdd(int checkX, int checkY)
    {
        if (checkX >= bottomLeft.x && checkX <= topRight.x && checkY >= bottomLeft.y && checkY <= topRight.y)
        {
            AStarNode NeighborNode = NodeArray[checkX - bottomLeft.x, checkY - bottomLeft.y];

            if (NeighborNode.IsWall || ClosedList.Contains(NeighborNode))
                return;

            if (allowDiagonal && NodeArray[CurNode.X - bottomLeft.x, checkY - bottomLeft.y].IsWall && NodeArray[checkX - bottomLeft.x, CurNode.Y - bottomLeft.y].IsWall)
                return;

            if (dontCrossCorner && (NodeArray[CurNode.X - bottomLeft.x, checkY - bottomLeft.y].IsWall || NodeArray[checkX - bottomLeft.x, CurNode.Y - bottomLeft.y].IsWall))
                return;

            int MoveCost = CurNode.G + (CurNode.X - checkX == 0 || CurNode.Y - checkY == 0 ? 10 : 14);

            if (MoveCost < NeighborNode.G || !OpenList.Contains(NeighborNode))
            {
                NeighborNode.G = MoveCost;
                NeighborNode.H = (Mathf.Abs(NeighborNode.X - TargetNode.X) + Mathf.Abs(NeighborNode.Y - TargetNode.Y)) * 10;
                NeighborNode.ParentNode = CurNode;

                if (!OpenList.Contains(NeighborNode))
                    OpenList.Add(NeighborNode);
            }
        }
    }

    bool _isDarawLine => _finalNodeList != null && _finalNodeList.Count != 0;
    void OnDrawGizmos()
    {
        if (_isDarawLine)
            for (int i = 0; i < _finalNodeList.Count - 1; i++)
                Gizmos.DrawLine(new Vector2(_finalNodeList[i].X, _finalNodeList[i].Y), new Vector2(_finalNodeList[i + 1].X, _finalNodeList[i + 1].Y));
    }
}
