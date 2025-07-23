using AutoBattle;
using System;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private TeamType _team;

    private bool _isDead;
    private bool _isBattleActive;
    private AStarAgent _aStarAgent;
    private RangeDetector _rangeDetector;
    private CombatComponent _combatComponent;
    private MovementComponent _movementComponent;
    private HealthComponent _healthComponent;
    private UnitStateMachine _stateMachine;
    private UnitStats _stats;
    private CapsuleCollider2D _capsuleCollider2D;
    private UnitDraggableBehaviour _draggableBehaviour;

    public event Action<Unit> OnDied;
    public bool IsDead => _isDead;
    public TeamType GetTeam() => _team;
    public UnitStats Stat => _stats;
    public AStarAgent Agent => _aStarAgent;

    public void Setup(TeamType team, UnitStatData statData, IGridManager gridManager)
    {
        _team = team;

        CacheComponents();
        InitializeComponents(statData, gridManager);
        RegisterEventHandlers();
    }

    private void CacheComponents()
    {
        _aStarAgent = GetComponent<AStarAgent>();
        _rangeDetector = GetComponentInChildren<RangeDetector>();
        _capsuleCollider2D = GetComponent<CapsuleCollider2D>();
        _draggableBehaviour = gameObject.AddComponent<UnitDraggableBehaviour>();
    }

    private void InitializeComponents(UnitStatData statData, IGridManager gridManager)
    {
        _stats = new UnitStats(statData);
        _capsuleCollider2D.enabled = true;
        _rangeDetector.Setup(Stat.Range);
        _stateMachine = new UnitStateMachine(this);
        _combatComponent = new CombatComponent(this, _rangeDetector, AutoBattleManager.Instance.Attack);
        _movementComponent = new MovementComponent(transform);
        _healthComponent = new HealthComponent(_stats);
        _draggableBehaviour.Setup(this, gridManager);
    }

    private void RegisterEventHandlers()
    {
        AutoBattleManager.Instance.StateController.BattleEntered.Add(() => _isBattleActive = true, 0);
        AutoBattleManager.Instance.StateController.VictoryEntered.Add(() => _isBattleActive = false, 0);
        AutoBattleManager.Instance.StateController.DefeatEntered.Add(() => _isBattleActive = false, 0);

        _aStarAgent.OnRequestTeamType += GetTeam;
        _rangeDetector.OnRequestTeamType += GetTeam;
        _movementComponent.OnEndMove += _aStarAgent.EndMove;
        _movementComponent.OnEndMove += _stateMachine.ChangeToIdle;
        _movementComponent.CancelMovementAction += _aStarAgent.ClearFllowing;
        _healthComponent.OnDied += _stateMachine.ChangeToDead;
        _combatComponent.OnAttackEnded += _stateMachine.ChangeToIdle;
        _aStarAgent.CrushOtherTeamAgent += _stateMachine.ChangeToIdle;
        _aStarAgent.OnPathCompleteAction += _stateMachine.ChangeToIdle;
        _aStarAgent.OnMove += _movementComponent.Move;
    }

    public void SetSnapTransform(Vector2Int positionInt)
    {
        transform.position = new Vector2(positionInt.x, positionInt.y);
        Agent.SetCurrentGridPosition(positionInt);
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
        _capsuleCollider2D.enabled = false;

        _combatComponent.CancelAttack();
        _movementComponent.CancelMovement();

        OnDied?.Invoke(this);

        //나중에 풀링으로 수정
        Destroy(gameObject, 1f);
    }

    public void OnEnterAttack()
    {
        _aStarAgent.ClearFllowing();
        _combatComponent.TryAttack();
    }

    public void OnEnterWalk()           => _aStarAgent.StartFollowPath();
    public bool ApplyDamage(int damage) => _healthComponent.ApplyDamage(damage);
}