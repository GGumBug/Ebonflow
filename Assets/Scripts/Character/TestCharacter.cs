using UnityEngine;

public class TestCharacter : MonoBehaviour, IAStarPathPoint
{
    public Vector2Int PathPoint => new Vector2Int(
        Mathf.RoundToInt(transform.position.x),
        Mathf.RoundToInt(transform.position.y)
    );
}
