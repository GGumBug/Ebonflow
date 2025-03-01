using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private TeamType team;
    [SerializeField] private AStarAgent _aStarAgent;

    private void OnMouseDown() 
    {
        _aStarAgent.FollowPath(team);
    }
}
