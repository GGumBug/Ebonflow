using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private TeamType team;
    [SerializeField] private AStarAgent aStarAgent;
    [SerializeField] private UnitRangeDetector unitRangeDetector;

    public TeamType Team => team;

    private void Awake()
    {
        aStarAgent.OnTargetTileOccupied += unitRangeDetector.IsOtherObjectInRange;
        aStarAgent.OnEnemyInRange += unitRangeDetector.IsEnemyInRange;
    }

    private void OnMouseDown() 
    {
        aStarAgent.BeginPathFollowing();
    }
}
