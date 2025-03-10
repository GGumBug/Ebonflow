using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private TeamType team;
    [SerializeField] private AStarAgent aStarAgent;
    [SerializeField] private RangeDetector rangeDetector;

    public TeamType GetTeam() => team;

    private void Awake()
    {
        aStarAgent.OnRequestTeamType += GetTeam;
        rangeDetector.OnRequestTeamType += GetTeam;
    }
}