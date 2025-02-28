using UnityEngine;

public interface IAStarGridSettings
{
    Vector2Int GridTopRight { get; }
    Vector2Int GridBottomLeft { get; }
}
