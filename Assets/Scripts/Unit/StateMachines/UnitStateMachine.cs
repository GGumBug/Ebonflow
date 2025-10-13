using System;

public class UnitStateMachine
{
    private readonly Unit _host;
    private readonly IUnitState _waitState;
    private readonly IUnitState _idleState;
    private readonly IUnitState _walkState;
    private readonly IUnitState _attackState;
    private readonly IUnitState _activeSkillState;
    private readonly IUnitState _deadState;

    public IUnitState CurrentState { get; private set; }

    public UnitStateMachine(Unit host)
    {
        _waitState = new WaitState();
        _idleState = new IdleState();
        _walkState = new WalkState();
        _attackState = new AttackState();
        _activeSkillState = new ActiveSkillState();
        _deadState = new DeadState();

        _host = host ?? throw new ArgumentNullException(nameof(host));
        CurrentState = _idleState;
        CurrentState.Enter(_host);
    }

    public void ChangeToWait() => ChangeState(_waitState);
    public void ChangeToIdle()   => ChangeState(_idleState);
    public void ChangeToWalk()   => ChangeState(_walkState);
    public void ChangeToAttack() => ChangeState(_attackState);
    public void ChangeToSkill() => ChangeState(_activeSkillState);
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