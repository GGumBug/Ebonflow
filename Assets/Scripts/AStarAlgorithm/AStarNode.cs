using UnityEngine;

[System.Serializable]
public class AStarNode
{
    public AStarNode(bool isBlock, int x, int y)
    {
        IsBlock = isBlock;
        X = x;
        Y = y;
    }


    [field: SerializeField] public bool IsBlock { get; private set; }
    [field: SerializeField] public int X { get; private set; }
    [field: SerializeField] public int Y { get; private set; }

    // G : 시작으로부터 이동했던 거리, H : |가로|+|세로| 장애물 무시하여 목표까지의 거리, F : G + H
    [field: SerializeField] public int G { get; set; }
    [field: SerializeField] public int H { get; set; }
    public AStarNode ParentNode { get; set; }

    public int F => G + H;
}