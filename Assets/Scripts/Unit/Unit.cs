using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private TeamType team;
    [SerializeField] private AStarAgent aStarAgent;
    [SerializeField] private RangeDetector rangeDetector;

    public TeamType Team => team;

    private void Awake()
    {
<<<<<<< Updated upstream
        aStarAgent.OnTargetTileOccupied += unitRangeDetector.IsOtherObjectInRange;
        aStarAgent.OnEnemyInRange += unitRangeDetector.IsEnemyInRange;
=======
        rangeDetector.OnRequestTeamType += () => { return Team; };
>>>>>>> Stashed changes
    }

    private void OnMouseDown() 
    {
        aStarAgent.BeginPathFollowing();
    }
}
