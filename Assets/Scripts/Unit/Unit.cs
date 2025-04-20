using System;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private TeamType team;

    private UnitStat _stat;
    private AStarAgent _aStarAgent;
    private RangeDetector _rangeDetector;
    private UnitStateController _unitStateController;

    public UnitStat Stat => _stat;
    public AStarAgent Agent => _aStarAgent;

    public TeamType GetTeam() => team;

    public void Setup(int unitID, int starLevel)
    {
        _stat = new UnitStat(unitID, starLevel);
        _aStarAgent = GetComponent<AStarAgent>();
        _rangeDetector = GetComponentInChildren<RangeDetector>();
        _unitStateController = GetComponent<UnitStateController>();

        _aStarAgent.OnRequestTeamType += GetTeam;
        _aStarAgent.OnEndWalk += () => _unitStateController.State = UnitState.Idle;
        _aStarAgent.OnAttackInitiated += CanAttack;
        _rangeDetector.OnRequestTeamType += GetTeam;

        Agent.ReserveCurrentGridCell();
    }

    public void StartBattle()
    {
        if (CanAttack())
            Attack();
        else
            Walk();
    }

    private bool CanAttack()
    {
        if (_rangeDetector.HasEnemies())
            return true;

        return false;
    }

    private void Attack()
    {
        _unitStateController.State = UnitState.Attack;
        Unit targetEnemy = _rangeDetector.GetClosestEnemy();

        Debug.Log($"{name} 이 {targetEnemy.name}을 공격합니다.");
    }

    private void Walk()
    {
        if (_unitStateController.State == UnitState.Walk)
            return;

        Debug.Log("이동을 시작합니다.");
        _unitStateController.State = UnitState.Walk;
        Agent.StartFollowPath();
    }
}