using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private TeamType _team;

    private bool _isBattleActive;
    private AStarAgent _aStarAgent;
    private RangeDetector _rangeDetector;
    private CombatComponent   _combatComponent;
    private MovementComponent _movementComponent;
    private HealthComponent   _healthComponent;
    private UnitStateMachine  _stateMachine;
    private UnitStats         _stats;

    public UnitStats Stat => _stats;
    public AStarAgent Agent => _aStarAgent;
    public TeamType GetTeam() => _team;

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
    }

    private void InitializeComponents(UnitStatData statData)
    {
        _stats = new UnitStats(statData);

        _rangeDetector.Setup(Stat.Range);

        _stateMachine = new UnitStateMachine(this);
        _combatComponent = new CombatComponent(this, _rangeDetector);
        _movementComponent = new MovementComponent(_aStarAgent);
        _healthComponent = new HealthComponent(_stats);
    }

    private void RegisterEventHandlers()
    {
        _aStarAgent.OnRequestTeamType       += GetTeam;
        _aStarAgent.OnAttackInitiated       += _combatComponent.CanAttack;
        _aStarAgent.OnEndWalk               += _stateMachine.ChangeToIdle;
        _aStarAgent.OnChangeToAttack        += _stateMachine.ChangeToAttack;
        
        _rangeDetector.OnRequestTeamType    += GetTeam;

        _combatComponent.OnAttackEnded      += TransitionToState;
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

        TransitionToState();
    }

    public void TransitionToState()
    {
        if (_combatComponent.CanAttack())
            _stateMachine.ChangeToAttack();
        else
            _stateMachine.ChangeToWalk();
    }

    private void Update()
    {
        if (!_isBattleActive)
            return;

        _stateMachine.Update();
    }

    public void OnEnterWalk()   => _movementComponent.StartWalking();
    public void OnEnterAttack() => _combatComponent.TryAttack();
    public void ApplyDamage(int damage) => _healthComponent.ApplyDamage(damage);
}