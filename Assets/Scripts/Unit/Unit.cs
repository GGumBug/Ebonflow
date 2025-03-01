using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private AStarAgent _aStarAgent;

    private void OnMouseDown() 
    {
        _aStarAgent.FollowPath();
    }
}
