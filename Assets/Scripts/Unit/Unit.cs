using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private TeamType team;
    [SerializeField] private AStarAgent aStarAgent;
    [SerializeField] private UnitRangeDetector unitRangeDetector;

    public TeamType Team => team;

    private void OnMouseDown() 
    {
        aStarAgent.BeginPathFollowing();
    }
}
