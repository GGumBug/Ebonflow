using System.Collections.Generic;
using UnityEngine;

public class AStarAgent : MonoBehaviour, IAStarPathPoint, IAStarPathFollower
{
    public Vector2Int PathPoint => new Vector2Int(
        Mathf.RoundToInt(transform.position.x),
        Mathf.RoundToInt(transform.position.y)
    );

    public void FollowPath(List<AStarNode> path)
    {
        throw new System.NotImplementedException();
    }
}
