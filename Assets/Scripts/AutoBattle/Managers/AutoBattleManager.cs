using System;
using UnityEngine;

namespace AutoBattle
{
    public class AutoBattleManager : Singleton<AutoBattleManager>
    {
        private DamageCalculator _damageCalculator;
        private RewardService _rewardService;
        private ManaGainService _manaGainService;

        public AutoBattleStateController StateController { get; private set; }

        public void Setup()
        {
            AutoBattleDataManager autoBattleDataManager = AutoBattleDataManager.Instance;
            _damageCalculator = new DamageCalculator();
            _rewardService = new RewardService(autoBattleDataManager.AutoBattlePlayerDataContext);
            _manaGainService = new ManaGainService();
            StateController = new AutoBattleStateController();

            AutoBattleUnitManager.Instance.OnTeamEliminated += HandleTeamEliminated;

            StateController.VictoryEntered.Add(() => autoBattleDataManager.AutoBattlePlayerDataContext.UpdateStreak(true), 0);
            StateController.VictoryEntered.Add(() => autoBattleDataManager.AutoBattleSceneDataContext.Stage.shouldResumeBattle = false, 0);
            StateController.VictoryEntered.Add(() => _rewardService.ApplyInterest(), 0);
            StateController.VictoryEntered.Add(() => autoBattleDataManager.AutoBattlePlayerDataContext.Save(), 1);
            StateController.VictoryEntered.Add(() => autoBattleDataManager.AutoBattleSceneDataContext.Save(), 1);
            StateController.VictoryEntered.Add(async () => await SceneLoadManager.Instance.LoadSceneAsyncWithLoadingUI<MapScene>(), 2);

            StateController.DefeatEntered.Add(() => autoBattleDataManager.AutoBattleSceneDataContext.Stage.shouldResumeBattle = false, 0);
            StateController.DefeatEntered.Add(() => autoBattleDataManager.AutoBattlePlayerDataContext.UpdateStreak(false), 0);
            StateController.DefeatEntered.Add(() => _rewardService.ApplyInterest(), 0);
            StateController.DefeatEntered.Add(() => autoBattleDataManager.AutoBattleSceneDataContext.Save(), 1);
            StateController.DefeatEntered.Add(() => autoBattleDataManager.AutoBattlePlayerDataContext.Save(), 1);
            StateController.DefeatEntered.Add(async () => await SceneLoadManager.Instance.LoadSceneAsyncWithLoadingUI<MapScene>(), 2);
        }

        public bool Attack(Unit attacker, Unit defender)
        {
            if (attacker == null) throw new ArgumentNullException(nameof(attacker));
            if (defender == null || defender.IsDead)
                throw new Exception("defender was null or dead.");

            // 1) 최종 데미지 계산
            var atkStats = attacker.Stat;
            var defStats = defender.Stat;
            int rawDamage = Mathf.Max(0, _damageCalculator.CalculateDamage(atkStats, defStats));

            // 2) HP 적용(실제 적용량 확보)
            int appliedDamage;
            bool killed = defender.Health.ApplyDamageAndGetApplied(rawDamage, out appliedDamage);

            // 3) 마나 충전(실제 적용량 기준)
            if (appliedDamage > 0)
            {
                _manaGainService?.OnDealDamage(attacker, appliedDamage);
                _manaGainService?.OnTakeDamage(defender, appliedDamage);
            }

            return killed;
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
            StateController.GameState = victory
                ? AutoBattleGameState.Victory
                : AutoBattleGameState.Defeat;

            Debug.Log(victory ? "승리!" : "패배…");
        }
    }
}