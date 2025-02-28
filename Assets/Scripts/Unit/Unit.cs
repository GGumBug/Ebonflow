using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private TeamType team;
    [SerializeField] private AStarAgent _aStarAgent;

    private void OnMouseDown() 
    {
        Debug.Log($"{name} is OnClicked!");
        _aStarAgent.FollowPath(team);
    }
}
