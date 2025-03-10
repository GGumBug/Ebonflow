using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private TeamType team;
    [SerializeField] private AStarAgent aStarAgent;
    [SerializeField] private RangeDetector rangeDetector;

    public TeamType Team => team;

    private void Awake()
    {
        rangeDetector.OnRequestTeamType += () => { return Team; };
    }
}
