using UnityEngine;

public static class Constants
{
    public const string LOADING_SCENE_NAME = "LoadingScene";

    public static readonly int BLOCK_LAYER = LayerMask.NameToLayer("Block");
    public static readonly int AGENT_LAYER = LayerMask.NameToLayer("Unit");
    public static readonly int START_AGENT_LAYER = LayerMask.NameToLayer("StartAgent");
    public static readonly int TARGET_AGENT_LAYER = LayerMask.NameToLayer("TargetAgent");

    public static readonly Vector2Int[] DIAGONAL_DIRECTIONS = new Vector2Int[]
    {
        new Vector2Int(1, 1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, -1),
        new Vector2Int(1, -1)
    };

    public static readonly Vector2Int[] ORTHOGONAL_DIRECTIONS = new Vector2Int[]
    {
        new Vector2Int(0, 1),
        new Vector2Int(1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0)
    };
}