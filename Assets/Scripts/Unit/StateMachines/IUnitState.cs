using UnityEngine;

public interface IUnitState
{
    void Enter(Unit unit);    // 상태 진입 시 1회 호출
    void Execute(Unit unit);  // 매 프레임(또는 타이밍)에 호출
    void Exit(Unit unit);     // 상태 이탈 시 1회 호출
}

public class WaitState : IUnitState
{
    public static readonly WaitState Instance = new WaitState();
    private WaitState() { }

    public void Enter(Unit unit)
    {
    }

    public void Execute(Unit unit)
    {
        
    }

    public void Exit(Unit unit)
    {
    }
}

public class IdleState : IUnitState
{
    public static readonly IdleState Instance = new IdleState();
    private IdleState() { }

    public void Enter(Unit unit)
    {
        // unit.Animator.Play("Idle");
    }

    public void Execute(Unit unit)
    {
        unit.TransitionToState();
    }

    public void Exit(Unit unit)
    {
        // 빠져나갈 때 정리할 게 있으면 여기에.
    }
}

public class WalkState : IUnitState
{
    public static readonly WalkState Instance = new WalkState();
    private WalkState() { }


    public void Enter(Unit unit)
    {
        // unit.Animator.Play("Walk");
        unit.OnEnterWalk();
    }

    public void Execute(Unit unit)
    {
        // if (unit.IsAtDestination)
        //     unit.StateMachine.ChangeState(new IdleState());
    }

    public void Exit(Unit unit)
    {
        // unit.StopPathFollowing();
    }
}

public class AttackState : IUnitState
{
    public static readonly AttackState Instance = new AttackState();
    private AttackState() { }

    public void Enter(Unit unit)
    {
        // unit.Animator.Play("Attack");
        unit.OnEnterAttack();
    }

    public void Execute(Unit unit)
    {
        // if (unit.IsAttackAnimationFinished)
        //     unit.StateMachine.ChangeState(new IdleState());
    }

    public void Exit(Unit unit)
    {
        
    }
}

public class DeadState : IUnitState
{
    public static readonly DeadState Instance = new DeadState();
    private DeadState() { }

    public void Enter(Unit unit)
    {
        // 애니메이션 재생, 콜라이더 비활성화 등
        unit.HandleDeath();
    }

    public void Execute(Unit unit) { }
    public void Exit(Unit unit) { }
}