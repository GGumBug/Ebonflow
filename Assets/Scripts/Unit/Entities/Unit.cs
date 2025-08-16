using AutoBattle;
using DeckSystem;
using System;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private TeamType _team;

    private int _instanceId = -1;
    private AStarAgent _aStarAgent;
    private RangeDetector _rangeDetector;
    private CombatComponent _combatComponent;
    private MovementComponent _movementComponent;
    private UnitStateMachine _stateMachine;
    private UnitStatData _statData;
    private UnitStats _stats;
    private CircleCollider2D _circleCollider2D;
    private UnitDraggableBehaviour _draggableBehaviour;
    private AutoBattleManager _autoBattleManager;
    private AutoBattleDataManager _autoBattleDataManager;

    private bool _battleHandlersSubscribed; // 중복 구독/해제 방지

    public UnitClass Class { get; private set; }
    public UnitOrigin Origin { get; private set; }
    public event Action<Unit> OnDied;
    public bool IsBattleActive { get; private set; }
    public bool IsDead { get; private set; }
    public TeamType GetTeam() => _team;
    public UnitStats Stat => _stats;
    public AStarAgent Agent => _aStarAgent;
    public Vector2Int CurrentGridPosition { get; private set; } // 현재 위치가 아닌, 그리드 시스템 상 위치
    public IGridManager CurrentGrid { get; private set; }
    public UnitSaleComponent SaleComponent { get; private set; }
    public HealthComponent Health { get; private set; }
    public ManaComponent Mana { get; private set; }

    public void SetInstanceId(int id) => _instanceId = id;

    private void Awake()
    {
        _autoBattleManager = AutoBattleManager.Instance;
        _autoBattleDataManager = AutoBattleDataManager.Instance;
    }

    public void Setup(TeamType team, UnitAggregate aggregate, IGridManager gridManager)
    {
        Class = aggregate.Data.Class;
        Origin = aggregate.Data.Origin;
        _team = team;
        _statData = aggregate.Stat;
        CacheComponents();
        InitializeComponents(gridManager);
        RegisterEventHandlers();
    }

    private void CacheComponents()
    {
        _aStarAgent = GetComponent<AStarAgent>();
        _rangeDetector = GetComponentInChildren<RangeDetector>();
        _circleCollider2D = GetComponent<CircleCollider2D>();
        _draggableBehaviour = gameObject.AddComponent<UnitDraggableBehaviour>();
    }

    private void InitializeComponents(IGridManager gridManager)
    {
        _stats = new UnitStats(_statData);
        _circleCollider2D.enabled = true;
        _rangeDetector.Setup(Stat.Range);
        _stateMachine = new UnitStateMachine(this);
        _combatComponent = new CombatComponent(this, _rangeDetector, _autoBattleManager.Attack);
        _movementComponent = new MovementComponent(transform);
        Health = new HealthComponent(_stats);
        Mana = new ManaComponent(_stats.MaxMana, 0);
        SetCurrentGrid(gridManager);
        _draggableBehaviour.Setup(this);
        int price = AutoBattleUnitManager.Instance.UnitStatRepository.Get(_statData.UnitId, _statData.StarLevel).Price;
        SaleComponent = new UnitSaleComponent(price, () => _instanceId);
    }

    public void RegisterUnit()
    {
        int instanceId = _autoBattleDataManager.AutoBattlePlayerDataContext.CreateUnit(_statData.UnitId, _statData.StarLevel);
        _instanceId = instanceId;
    }

    public void RegisterPlacement(GridType gridType)
    {
        _autoBattleDataManager.AutoBattlePlayerDataContext.UpsertPlacement(_instanceId, gridType, Agent.PathPoint.x, Agent.PathPoint.y);
    }

    private void RegisterEventHandlers()
    {
        // 메서드 그룹/이름 있는 핸들러만 사용 (람다 X)
        _aStarAgent.OnRequestTeamType += GetTeam;
        _rangeDetector.OnRequestTeamType += GetTeam;

        _movementComponent.OnEndMove += _aStarAgent.EndMove;
        _movementComponent.OnEndMove += _stateMachine.ChangeToIdle;
        _movementComponent.CancelMovementAction += _aStarAgent.ClearFllowing;

        Health.OnDied += _stateMachine.ChangeToDead;
        _combatComponent.OnAttackEnded += _stateMachine.ChangeToIdle;

        _aStarAgent.GetCurrentGridPositionAction += OnRequestCurrentGridPos;
        _aStarAgent.SetCurrentGridPositionAction += SetCurrentGridPosition;
        _aStarAgent.CrushOtherTeamAgent += _stateMachine.ChangeToIdle;
        _aStarAgent.OnPathCompleteAction += _stateMachine.ChangeToIdle;
        _aStarAgent.OnMove += _movementComponent.Move;

        SaleComponent.RequestReleaseAndPool += ReleaseAndPool;
        SaleComponent.RequestCardData += MakeCardData;
    }

    public void UnregisterEventHandlers()
    {
        _aStarAgent.OnRequestTeamType -= GetTeam;
        _rangeDetector.OnRequestTeamType -= GetTeam;

        _movementComponent.OnEndMove -= _aStarAgent.EndMove;
        _movementComponent.OnEndMove -= _stateMachine.ChangeToIdle;
        _movementComponent.CancelMovementAction -= _aStarAgent.ClearFllowing;

        Health.OnDied -= _stateMachine.ChangeToDead;
        _combatComponent.OnAttackEnded -= _stateMachine.ChangeToIdle;

        _aStarAgent.GetCurrentGridPositionAction -= OnRequestCurrentGridPos;
        _aStarAgent.SetCurrentGridPositionAction -= SetCurrentGridPosition;
        _aStarAgent.CrushOtherTeamAgent -= _stateMachine.ChangeToIdle;
        _aStarAgent.OnPathCompleteAction -= _stateMachine.ChangeToIdle;
        _aStarAgent.OnMove -= _movementComponent.Move;

        SaleComponent.RequestReleaseAndPool -= ReleaseAndPool;
        SaleComponent.RequestCardData -= MakeCardData;
    }

    public void SetCurrentGridPosition(Vector2Int positionInt)
    {
        CurrentGridPosition = positionInt;
    }

    public void SetSnapTransform(Vector2Int positionInt)
    {
        transform.position = new Vector2(positionInt.x, positionInt.y);
        SetCurrentGridPosition(positionInt);
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
        // 배틀 상태 핸들러 해제(중복 호출 안전)
        UnsubscribeBattleStateHandlers();

        CurrentGrid.RemoveUnit(CurrentGridPosition, this);
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
        if (_battleHandlersSubscribed) return;

        var ctrl = _autoBattleManager.StateController;
        ctrl.BattleEntered.Add(OnBattleEntered_RegistPosition, priority: 3);
        ctrl.BattleEntered.Add(OnBattleEntered_Activate, priority: 2);
        ctrl.BattleEntered.Add(OnBattleEntered_ScanRange, priority: 1);
        ctrl.VictoryEntered.Add(OnBattleExited_Deactivate, priority: 1);
        ctrl.DefeatEntered.Add(OnBattleExited_Deactivate, priority: 1);

        _battleHandlersSubscribed = true;
    }

    public void UnsubscribeBattleStateHandlers()
    {
        if (!_battleHandlersSubscribed) return;

        var ctrl = _autoBattleManager.StateController;
        ctrl.BattleEntered.Remove(OnBattleEntered_RegistPosition);
        ctrl.BattleEntered.Remove(OnBattleEntered_Activate);
        ctrl.BattleEntered.Remove(OnBattleEntered_ScanRange);
        ctrl.VictoryEntered.Remove(OnBattleExited_Deactivate);
        ctrl.DefeatEntered.Remove(OnBattleExited_Deactivate);

        _battleHandlersSubscribed = false;
    }

    private CardData MakeCardData()
    {
        var repo = AutoBattleUnitManager.Instance.UnitStatRepository.Get(_stats.BaseStats.UnitId, _stats.BaseStats.StarLevel);

        CardData cardData = new CardData(
            repo.Data.UnitTier,
            repo.Price,
            repo.Data.UnitId,
            repo.Stat.StarLevel
        );
        return cardData;
    }

    public void OnEnterWalk() => _aStarAgent.StartFollowPath();
    public void SetCurrentGrid(IGridManager grid) => CurrentGrid = grid;

    // =========================
    // Named Handlers (람다 금지)
    // =========================

    /// <summary>배틀 진입 시: 현재 경로상 위치 등록</summary>
    private void OnBattleEntered_RegistPosition() => Agent.RegistPosition();

    /// <summary>배틀 진입 시: 전투 활성화</summary>
    private void OnBattleEntered_Activate() => IsBattleActive = true;

    /// <summary>배틀 진입 시: 사거리 내 적 탐색</summary>
    private void OnBattleEntered_ScanRange() => _rangeDetector.FindEnemiesInRange();

    /// <summary>승리/패배 시: 전투 비활성화</summary>
    private void OnBattleExited_Deactivate() => IsBattleActive = false;

    /// <summary>A*가 현재 그리드 좌표를 요청할 때 반환</summary>
    private Vector2Int OnRequestCurrentGridPos() => CurrentGridPosition;
}
