using AutoBattle;
using System;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private TeamType _team;

    private AStarAgent _aStarAgent;
    private RangeDetector _rangeDetector;
    private CombatComponent _combatComponent;
    private MovementComponent _movementComponent;
    private HealthComponent _healthComponent;
    private UnitStateMachine _stateMachine;
    private UnitStats _stats;
    private CircleCollider2D _circleCollider2D;
    private UnitDraggableBehaviour _draggableBehaviour;
    private AutoBattleManager _autoBattleManager;
    private UnitSaleComponent _unitSaleComponent;

    public event Action<Unit> OnDied;
    public bool IsBattleActive { get; private set; }
    public bool IsDead { get; private set; }
    public TeamType GetTeam() => _team;
    public UnitStats Stat => _stats;
    public AStarAgent Agent => _aStarAgent;
    public IGridManager CurrentGrid { get; private set; }

    private void Awake()
    {
        _autoBattleManager = AutoBattleManager.Instance;
    }

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
        _circleCollider2D = GetComponent<CircleCollider2D>();
        _draggableBehaviour = gameObject.AddComponent<UnitDraggableBehaviour>();
    }

    private void InitializeComponents(UnitStatData statData, IGridManager gridManager)
    {
        _stats = new UnitStats(statData);
        _circleCollider2D.enabled = true;
        _rangeDetector.Setup(Stat.Range);
        _stateMachine = new UnitStateMachine(this);
        _combatComponent = new CombatComponent(this, _rangeDetector, _autoBattleManager.Attack);
        _movementComponent = new MovementComponent(transform);
        _healthComponent = new HealthComponent(_stats);
        SetCurrentGrid(gridManager);
        _draggableBehaviour.Setup(this);
        int price = AutoBattleUnitManager.Instance.UnitStatRepository.Get(statData.UnitId, statData.StarLevel).Price;
        _unitSaleComponent = new UnitSaleComponent(price);
    }

    private void RegisterEventHandlers()
    {
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
        _unitSaleComponent.RequestReleaseAndPool += ReleaseAndPool;
    }

    public void UnregisterEventHandlers()
    {
        _aStarAgent.OnRequestTeamType -= GetTeam;
        _rangeDetector.OnRequestTeamType -= GetTeam;
        _movementComponent.OnEndMove -= _aStarAgent.EndMove;
        _movementComponent.OnEndMove -= _stateMachine.ChangeToIdle;
        _movementComponent.CancelMovementAction -= _aStarAgent.ClearFllowing;
        _healthComponent.OnDied -= _stateMachine.ChangeToDead;
        _combatComponent.OnAttackEnded -= _stateMachine.ChangeToIdle;
        _aStarAgent.CrushOtherTeamAgent -= _stateMachine.ChangeToIdle;
        _aStarAgent.OnPathCompleteAction -= _stateMachine.ChangeToIdle;
        _aStarAgent.OnMove -= _movementComponent.Move;
        _unitSaleComponent.RequestReleaseAndPool -= ReleaseAndPool;
    }

    public void SetSnapTransform(Vector2Int positionInt)
    {
        transform.position = new Vector2(positionInt.x, positionInt.y);
        Agent.SetCurrentGridPosition(positionInt);
    }

    private void Update()
    {
        if (!IsBattleActive || IsDead)
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
        IsDead = true;
        _circleCollider2D.enabled = false;
        _combatComponent.CancelAttack();
        _movementComponent.CancelMovement();
        OnDied?.Invoke(this);

        ReleaseAndPool();
    }

    private void ReleaseAndPool()
    {
        CurrentGrid.RemoveUnit(Agent.CurrentGridPosition, this);

        UnregisterEventHandlers();

        PoolManager.Instance.Push(GetComponent<Poolable>());
    }

    public void OnEnterAttack()
    {
        _aStarAgent.ClearFllowing();
        _combatComponent.TryAttack();
    }

    public void SubscribeBattleStateHandlers()
    {
        var ctrl = _autoBattleManager.StateController;
        ctrl.BattleEntered.Add(() => Agent.RegistPosition(), priority: 2);
        ctrl.BattleEntered.Add(() => IsBattleActive = true, priority: 1);
        ctrl.BattleEntered.Add(() => _rangeDetector.FindEnemiesInRange(), priority: 0);
        ctrl.VictoryEntered.Add(() => IsBattleActive = false, priority: 1);
        ctrl.DefeatEntered.Add(() => IsBattleActive = false, priority: 1);
    }

    public void UnsubscribeBattleStateHandlers()
    {
        var ctrl = _autoBattleManager.StateController;
        ctrl.BattleEntered.Remove(() => Agent.RegistPosition());
        ctrl.BattleEntered.Remove(() => IsBattleActive = true);
        ctrl.BattleEntered.Add(() => _rangeDetector.FindEnemiesInRange());
        ctrl.VictoryEntered.Remove(() => IsBattleActive = false);
        ctrl.DefeatEntered.Remove(() => IsBattleActive = false);
    }

    public void OnEnterWalk()               => _aStarAgent.StartFollowPath();
    public bool ApplyDamage(int damage)     => _healthComponent.ApplyDamage(damage);
    public void SetCurrentGrid(IGridManager grid) => CurrentGrid = grid;
}