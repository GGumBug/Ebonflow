using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private TeamType team;

    public TeamType Team { get; private set; }
    public AStarAgent Agent {get; private set;}

    private void Awake() 
    {
        Agent = gameObject.AddComponent<AStarAgent>();
    }
}
