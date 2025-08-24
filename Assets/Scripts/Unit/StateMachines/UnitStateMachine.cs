using System;

public class UnitStateMachine
{
    private readonly Unit _host;
    private readonly IUnitState _waitState = WaitState.Instance;
    private readonly IUnitState _idleState  = IdleState.Instance;
    private readonly IUnitState _walkState  = WalkState.Instance;
    private readonly IUnitState _attackState = AttackState.Instance;
    private readonly IUnitState _deadState = DeadState.Instance;

    public IUnitState CurrentState { get; private set; }

    public UnitStateMachine(Unit host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        CurrentState = _idleState;
        CurrentState.Enter(_host);
    }

    public void ChangeToWait() => ChangeState(_waitState);
    public void ChangeToIdle()   => ChangeState(_idleState);
    public void ChangeToWalk()   => ChangeState(_walkState);
    public void ChangeToAttack() => ChangeState(_attackState);
    public void ChangeToDead() => ChangeState(_deadState);

    public void Update()         => CurrentState.Execute(_host);

    private void ChangeState(IUnitState newState)
    {
        if (CurrentState == newState) return;

        CurrentState.Exit(_host);
        CurrentState = newState;
        CurrentState.Enter(_host);
    }
}