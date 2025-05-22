using System;
using UnityEngine;
using AutoBattle;

public class Unit : MonoBehaviour
{
    [SerializeField] private TeamType _team;

    private bool                    _isDead;
    private bool                    _isBattleActive;
    private AStarAgent              _aStarAgent;
    private RangeDetector           _rangeDetector;
    private CombatComponent         _combatComponent;
    private MovementComponent       _movementComponent;
    private HealthComponent         _healthComponent;
    private UnitStateMachine        _stateMachine;
    private UnitStats               _stats;
    private CircleCollider2D        _circleCollider2D;

    public event Action<Unit>       OnDied;
    public bool IsDead          =>  _isDead;
    public TeamType GetTeam()   =>  _team;
    public UnitStats Stat       =>  _stats;
    public AStarAgent Agent     =>  _aStarAgent;

    public void Setup(TeamType team, UnitStatData statData)
    {
        _team = team;
        
        CacheComponents();
        InitializeComponents(statData);
        RegisterEventHandlers();
        ReserveGridCell();
    }

    private void CacheComponents()
    {
        _aStarAgent          = GetComponent<AStarAgent>();
        _rangeDetector       = GetComponentInChildren<RangeDetector>();
        _circleCollider2D    = GetComponent<CircleCollider2D>();
    }

    private void InitializeComponents(UnitStatData statData)
    {
        _stats = new UnitStats(statData);
        _circleCollider2D.enabled = true;
        _rangeDetector.Setup(Stat.Range);
        _stateMachine = new UnitStateMachine(this);
        _combatComponent = new CombatComponent(this, _rangeDetector, AutoBattleManager.Instance.Attack);
        _movementComponent = new MovementComponent(transform);
        _healthComponent = new HealthComponent(_stats);
    }

    private void RegisterEventHandlers()
    {
        AutoBattleManager.Instance.OnBattleStarted += () => _isBattleActive = true;
        AutoBattleManager.Instance.OnBattleEnded += () => _isBattleActive = false;

        _aStarAgent.OnRequestTeamType               += GetTeam;
        _rangeDetector.OnRequestTeamType            += GetTeam;
        _movementComponent.OnEndMove                += _aStarAgent.EndMove;
        _movementComponent.OnEndMove                += _stateMachine.ChangeToIdle;
        _movementComponent.CancelMovementAction     += _aStarAgent.ClearFllowing;
        _healthComponent.OnDied                     += _stateMachine.ChangeToDead;
        _combatComponent.OnAttackEnded              += _stateMachine.ChangeToIdle;
        _aStarAgent.CrushOtherTeamAgent             += _stateMachine.ChangeToIdle;
        _aStarAgent.OnPathCompleteAction            += _stateMachine.ChangeToIdle;
        _aStarAgent.OnMove                          += _movementComponent.Move;
    }

    /// <summary>
    /// 현재 위치를 그리드 상에 예약(block) 처리합니다.
    /// </summary>
    private void ReserveGridCell()
    {
        Agent.ReserveCurrentGridCell();
    }

    public void StartBattle()
    {
        _isBattleActive = true;
    }

    private void Update()
    {
        if (!_isBattleActive || _isDead)
            return;

        _stateMachine.Update();
    }

    public void TransitionToState()
    {
        if (_combatComponent.CanAttack())
            _stateMachine.ChangeToAttack();
        else
            _stateMachine.ChangeToWalk();
    }

    public void HandleDeath()
    {
        _isDead = true;
        Agent.UnreserveCurrentGridCell();
        _circleCollider2D.enabled = false;

        _combatComponent.CancelAttack();
        _movementComponent.CancelMovement();

        OnDied?.Invoke(this);
        
        //나중에 풀링으로 수정
        Destroy(gameObject, 1f);
    }

    public void OnEnterWalk()           => _aStarAgent.StartFollowPath();
    public void OnEnterAttack()         => _combatComponent.TryAttack();
    public bool ApplyDamage(int damage) => _healthComponent.ApplyDamage(damage);
}