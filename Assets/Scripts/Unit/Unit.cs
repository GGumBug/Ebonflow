using System;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private TeamType _team;

    private bool _isBattleActive;
    private UnitStats _stats;
    private AStarAgent _aStarAgent;
    private RangeDetector _rangeDetector;
    private UnitStateMachine _stateMachine;

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

    private void InitializeComponents(UnitStatData statData)
    {
        _stats = new UnitStats(statData);
        _stateMachine = new UnitStateMachine(this);
    }

    private void CacheComponents()
    {
        _aStarAgent          = GetComponent<AStarAgent>();
        _rangeDetector       = GetComponentInChildren<RangeDetector>();
    }

    private void RegisterEventHandlers()
    {
        _aStarAgent.OnRequestTeamType       += GetTeam;
        _aStarAgent.OnAttackInitiated       += CanAttack;
        _aStarAgent.OnEndWalk               += _stateMachine.ChangeToIdle;
        _aStarAgent.OnChangeToAttack        += _stateMachine.ChangeToAttack;
        
        _rangeDetector.OnRequestTeamType    += GetTeam;
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

        TransitionToBattleState();
    }

    public void TransitionToBattleState()
    {
        if (CanAttack())
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

    private bool CanAttack()
    {
        if (_rangeDetector.HasEnemies())
            return true;

        return false;
    }

    public void Attack()
    {
        Unit targetEnemy = _rangeDetector.GetClosestEnemy();

        Debug.Log($"{name} 이 {targetEnemy.name}을 공격합니다.");
    }

    public void Walk()
    {   
        Agent.StartFollowPath();
    }
}