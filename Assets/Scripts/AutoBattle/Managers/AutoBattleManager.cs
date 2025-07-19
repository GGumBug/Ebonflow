using System;
using UnityEngine;

namespace AutoBattle
{
    public class AutoBattleManager : Singleton<AutoBattleManager>
    {
        private DamageCalculator _damageCalculator;

        public event Action OnBattleStarted;
        public event Action OnBattleEnded;

        public AutoBattleStateController StateController { get; private set; }

        public void Setup()
        {
            _damageCalculator = new DamageCalculator();
            StateController = new AutoBattleStateController();
            AutoBattleUnitManager.Instance.OnTeamEliminated += HandleTeamEliminated;
            StateController.OnBattlePhase += OnBattleStarted;
        }

        public bool Attack(Unit attacker, Unit defender)
        {
            if (defender == null || defender.IsDead)
                throw new Exception("defender was null or dead.");

            var atkStats = attacker.Stat;
            var defStats = defender.Stat;

            int damage = _damageCalculator.CalculateDamage(atkStats, defStats);
            return defender.ApplyDamage(damage);
        }

        private void HandleTeamEliminated(TeamType eliminatedTeam)
        {
            if (eliminatedTeam == TeamType.Ally)
                BattleEnded(victory: false);
            else
                BattleEnded(victory: true);
        }

        private void BattleEnded(bool victory)
        {
            OnBattleEnded?.Invoke();

            StateController.GameState = victory
                ? AutoBattleGameState.Victory
                : AutoBattleGameState.Defeat;

            Debug.Log(victory ? "승리!" : "패배…");
        }
    }
}