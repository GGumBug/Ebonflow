using System;
using UnityEngine;

public class AutoBattleManager : Singleton<AutoBattleManager>
{
    private DamageCalculator _damageCalculator;
    
    public event Action OnBattleStarted;

    public AutoBattleStateController StateController { get; private set; }

    public void Setup()
    {   
        _damageCalculator = new DamageCalculator();
        StateController = new AutoBattleStateController();
    }
    
    public void StartBattle()
    {
        StateController.GameState = AutoBattleGameState.InProgress;

        OnBattleStarted?.Invoke();
    }

    public void Attack(Unit attacker, Unit defender)
    {
        if (defender == null || defender.IsDead)
            throw new Exception("defender was null or dead.");

        var atkStats = attacker.Stat;
        var defStats = defender.Stat;

        int damage = _damageCalculator.CalculateDamage(atkStats, defStats);
        defender.ApplyDamage(damage);
    }
}
