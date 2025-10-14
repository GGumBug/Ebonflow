using AutoBattle;
using CombatSystem;
using Cysharp.Threading.Tasks;
using DeckSystem;
using System;
using System.Threading;
using UnityEngine;

public class Unit : MonoBehaviour, IUpdateObserver, IAttacker, IVictim
{
    [SerializeField] private UnitModel model;
    [SerializeField] private UIUnitStatBars uIUnitStatBars;
    [SerializeField] private TeamType team;

    private int _instanceId = -1;
    private bool _battleHandlersSubscribed; // 중복 구독/해제 방지
    private bool _isActiveSkill = false;
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
    private CombatManager _combatManager;
    private AutoBattleDataManager _autoBattleDataManager;
    private CancellationTokenSource _deathCts;

    public event Action<IVictim> OnDied;

    public UnitClass Class { get; private set; }
    public UnitOrigin Origin { get; private set; }
    public int AttackSkillID { get; private set; }
    public int ActiveSkillID { get; private set; }
    public bool IsBattleActive { get; private set; }
    public bool IsDead { get; private set; }
    public int TeamId => (int)team;
    public UnitStateMachine StateMachine => _stateMachine;
    public UnitModel Model => model;
    public UnitStats Stat => _stats;
    public AStarAgent Agent => _aStarAgent;
    public Vector3 Position => transform.position; // 현재 오브젝트 위치
    public Vector2Int CurrentGridPosition { get; private set; } // 현재 위치가 아닌, 그리드 시스템 상 위치
    public IGridManager CurrentGrid { get; private set; }
    public UnitSaleComponent SaleComponent { get; private set; }
    public HealthComponent Health { get; private set; }
    public ManaComponent Mana { get; private set; }

    public TeamType GetTeam() => team;
    public int GetTeamID() => TeamId;
    public void SetInstanceId(int id) => _instanceId = id;

    private void Awake()
    {
        _autoBattleManager = AutoBattleManager.Instance;
        _autoBattleDataManager = AutoBattleDataManager.Instance;
        _combatManager = CombatManager.Instance;
    }

    public void Setup(TeamType team, UnitAggregate aggregate, IGridManager gridManager)
    {
        CurrentGridPosition = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        model.SetUnitDirection(Vector2.down);
        model.SetAnimatorController(aggregate.Data.UnitAnimatorKey);

        Class = aggregate.Data.Class;
        Origin = aggregate.Data.Origin;
        AttackSkillID = aggregate.Data.AttackSkillID;
        ActiveSkillID = aggregate.Data.ActiveSkillID;

        this.team = team;
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
        Mana = new ManaComponent(_stats.MaxMana, 0);
        _combatComponent = new CombatComponent(this, _rangeDetector, _combatManager.Trigger, Mana);
        _movementComponent = new MovementComponent(transform);
        Health = new HealthComponent(_stats);
        uIUnitStatBars.Bind(Health, Mana);

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
        _rangeDetector.OnRequestTeamId += GetTeamID;

        _movementComponent.OnStartMove += model.SetUnitDirection;
        _movementComponent.OnEndMove += _aStarAgent.EndMove;
        _movementComponent.OnEndMove += _stateMachine.ChangeToIdle;
        _movementComponent.CancelMovementAction += _aStarAgent.ClearFllowing;

        Health.OnDied += _stateMachine.ChangeToDead;
        _combatComponent.OnAttackStarted += OnEnterAttack;
        _combatComponent.OnAttackEnded += _stateMachine.ChangeToIdle;

        _aStarAgent.GetCurrentGridPositionAction += OnRequestCurrentGridPos;
        _aStarAgent.SetCurrentGridPositionAction += SetCurrentGridPosition;
        _aStarAgent.CrushOtherTeamAgent += _stateMachine.ChangeToIdle;
        _aStarAgent.OnPathCompleteAction += _stateMachine.ChangeToIdle;
        _aStarAgent.OnMove += _movementComponent.Move;
        _aStarAgent.RequestEnterWaitState += _stateMachine.ChangeToWait;
        _aStarAgent.RequestExitWaitState += _stateMachine.ChangeToIdle;

        SaleComponent.RequestReleaseAndPool += ReleaseAndPool;
        SaleComponent.RequestCardData += MakeCardData;
    }

    public void UnregisterEventHandlers()
    {
        _aStarAgent.OnRequestTeamType -= GetTeam;
        _rangeDetector.OnRequestTeamId -= GetTeamID;

        _movementComponent.OnStartMove -= model.SetUnitDirection;
        _movementComponent.OnEndMove -= _aStarAgent.EndMove;
        _movementComponent.OnEndMove -= _stateMachine.ChangeToIdle;
        _movementComponent.CancelMovementAction -= _aStarAgent.ClearFllowing;

        Health.OnDied -= _stateMachine.ChangeToDead;
        _combatComponent.OnAttackStarted -= OnEnterAttack;
        _combatComponent.OnAttackEnded -= _stateMachine.ChangeToIdle;

        _aStarAgent.GetCurrentGridPositionAction -= OnRequestCurrentGridPos;
        _aStarAgent.SetCurrentGridPositionAction -= SetCurrentGridPosition;
        _aStarAgent.CrushOtherTeamAgent -= _stateMachine.ChangeToIdle;
        _aStarAgent.OnPathCompleteAction -= _stateMachine.ChangeToIdle;
        _aStarAgent.OnMove -= _movementComponent.Move;
        _aStarAgent.RequestEnterWaitState -= _stateMachine.ChangeToWait;
        _aStarAgent.RequestExitWaitState -= _stateMachine.ChangeToIdle;

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

    public void ObservedUpdate()
    {
        if (!IsBattleActive || IsDead)
            return;

        _stateMachine.Update();
    }

    public void TransitionToState()
    {
        if (_combatComponent.TryAttack(out _isActiveSkill))
        {
            if (_isActiveSkill)
                _stateMachine.ChangeToSkill();
            else
                _stateMachine.ChangeToAttack();
        }
        else
            _stateMachine.ChangeToWalk();
    }

    public void HandleDeath()
    {
        _deathCts?.Cancel();
        _deathCts?.Dispose();
        _deathCts = new CancellationTokenSource();
        CancellationToken token = _deathCts.Token;

        IsDead = true;
        _circleCollider2D.enabled = false;
        _combatComponent.CancelAttack();
        _movementComponent.CancelMovement();
        OnDied?.Invoke(this);

        HandleDeathSequence(token).Forget();
    }

    private async UniTask HandleDeathSequence(CancellationToken token)
    {
        try
        {
            await UniTask.Delay(
                TimeSpan.FromSeconds(1.5f),
                DelayType.DeltaTime,
                PlayerLoopTiming.Update,
                token // CancellationToken 주입
            );

            ReleaseAndPool();
        }
        catch (OperationCanceledException)
        {
            Debug.Log("사망 후 풀 반환 작업이 취소되었습니다.");
        }
    }

    private void ReleaseAndPool()
    {
        UnsubscribeBattleStateHandlers();
        UnregisterEventHandlers();

        CurrentGrid.RemoveUnit(CurrentGridPosition, this);
        PoolManager.Instance.Push(GetComponent<Poolable>());
    }

    public void OnEnterAttack()
    {
        _aStarAgent.ClearFllowing();
    }

    public void SubscribeBattleStateHandlers()
    {
        if (_battleHandlersSubscribed) return;

        var ctrl = _autoBattleManager.StateController;
        ctrl.BattleEntered.Add(OnBattleEntered_RegistPosition, priority: 0);
        ctrl.BattleEntered.Add(OnBattleEntered_Activate, priority: 1);
        ctrl.BattleEntered.Add(OnBattleEntered_ScanRange, priority: 2);
        ctrl.VictoryEntered.Add(OnBattleExited_Deactivate, priority: 0);
        ctrl.DefeatEntered.Add(OnBattleExited_Deactivate, priority: 0);

        _battleHandlersSubscribed = true;

        UpdateManager.Instance.RegisterObserver(this);
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

    private void OnDisable()
    {
        UpdateManager.Instance.UnRegisterObserver(this);
    }

    private void OnDestroy()
    {
        _combatComponent?.Dispose();
        _deathCts?.Cancel();
        _deathCts?.Dispose();
    }
}
