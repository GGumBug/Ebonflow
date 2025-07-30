[System.Serializable]
public class AStarNode
{
    public AStarNode(bool isBlock, bool isPlaceable, int x, int y)
    {
        _isBlock = isBlock;
        IsPlaceable = isPlaceable;
        X = x;
        Y = y;

        Agent = null;
    }

    private bool _isBlock;

    public bool IsPlaceable { get; private set; }
    public int X { get; private set; }
    public int Y { get; private set; }
    public AStarAgent Agent { get; set; }

    // G : 시작으로부터 이동했던 거리, H : |가로|+|세로| 장애물 무시하여 목표까지의 거리, F : G + H
    public int G { get; set; }
    public int H { get; set; }

    public bool GetBlock { get { return _isBlock; } }
    public bool SetBlock  { set { _isBlock = value; } }
    public AStarNode ParentNode { get; set; }

    public int F => G + H;
}