using UnityEngine;

public interface IGridSettings
{
    Vector2Int GridTopRight { get; }
    Vector2Int GridBottomLeft { get; }
}
