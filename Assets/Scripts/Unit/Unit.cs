using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private AStarAgent aStarAgent;
    [SerializeField] private RangeDetector rangeDetector;

    private void OnMouseDown() 
    {
        aStarAgent.FollowPath();
    }
}
