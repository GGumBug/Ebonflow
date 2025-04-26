public class MovementComponent
{
    private AStarAgent _agent;

    public MovementComponent(AStarAgent agent)
    {
        _agent = agent;
    }

    public void StartWalking() => _agent.StartFollowPath();
    public void CancelMovement() => _agent.CancelMovement();
}
