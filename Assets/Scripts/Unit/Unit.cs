using System;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private TeamType team;
    [SerializeField] private AStarAgent aStarAgent;
    [SerializeField] private RangeDetector rangeDetector;
    [SerializeField] private UnitStateController unitStateController;

    public AStarAgent Agent => aStarAgent;

    public TeamType GetTeam() => team;

    private void Awake()
    {
        aStarAgent.OnRequestTeamType += GetTeam;
        aStarAgent.OnBeginWalk += () => unitStateController.State = UnitState.Walk;
        aStarAgent.OnAttackInitiated += CanAttack;
        rangeDetector.OnRequestTeamType += GetTeam;
    }

    public void Setup()
    {
        Agent.ReserveCurrentGridCell();
    }

    public void StartBattle()
    {
        if (CanAttack())
            Attack();
        else
            Agent.StartFollowPath();
    }

    private bool CanAttack()
    {
        if (rangeDetector.HasEnemies())
            return true;

        return false;
    }

    private void Attack()
    {
        unitStateController.State = UnitState.Attack;
        Unit targetEnemy = rangeDetector.GetClosestEnemy();

        Debug.Log($"{name} 이 {targetEnemy.name}을 공격합니다.");
    }
}