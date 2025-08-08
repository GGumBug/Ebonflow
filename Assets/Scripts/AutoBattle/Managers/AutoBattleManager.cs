using RoguelikeMap;
using System;
using UnityEngine;

namespace AutoBattle
{
    public class AutoBattleManager : Singleton<AutoBattleManager>
    {
        private DamageCalculator _damageCalculator;
        private RewardService _rewardService;

        public AutoBattleStateController StateController { get; private set; }

        public void Setup()
        {
            AutoBattleDataManager autoBattleDataManager = AutoBattleDataManager.Instance;
            _damageCalculator = new DamageCalculator();
            _rewardService = new RewardService(autoBattleDataManager.AutoBattlePlayerDataContext);
            StateController = new AutoBattleStateController();

            AutoBattleUnitManager.Instance.OnTeamEliminated += HandleTeamEliminated;

            StateController.VictoryEntered.Add(() => MapSaveLoadManager.Instance.SaveMap(), 0);
            StateController.VictoryEntered.Add(() => autoBattleDataManager.AutoBattleSceneDataContext.Stage.shouldResumeBattle = false, 0);
            StateController.VictoryEntered.Add(() => autoBattleDataManager.AutoBattlePlayerDataContext.UpdateStreak(true), 0);
            StateController.VictoryEntered.Add(() => autoBattleDataManager.AutoBattlePlayerDataContext.Save(), 0);
            StateController.VictoryEntered.Add(() => autoBattleDataManager.AutoBattleSceneDataContext.Save(), 1);
            StateController.VictoryEntered.Add(() => _rewardService.ApplyInterest(), 2);
            StateController.VictoryEntered.Add(async () => await SceneLoadManager.Instance.LoadSceneAsyncWithLoadingUI<MapScene>(), 2);

            StateController.DefeatEntered.Add(() => MapSaveLoadManager.Instance.SaveMap(), 0);
            StateController.DefeatEntered.Add(() => autoBattleDataManager.AutoBattleSceneDataContext.Stage.shouldResumeBattle = false, 0);
            StateController.DefeatEntered.Add(() => autoBattleDataManager.AutoBattlePlayerDataContext.Save(), 0);
            StateController.DefeatEntered.Add(() => autoBattleDataManager.AutoBattleSceneDataContext.Save(), 1);
            StateController.DefeatEntered.Add(() => autoBattleDataManager.AutoBattlePlayerDataContext.UpdateStreak(false), 0);
            StateController.DefeatEntered.Add(() => _rewardService.ApplyInterest(), 1);
            StateController.DefeatEntered.Add(async () => await SceneLoadManager.Instance.LoadSceneAsyncWithLoadingUI<MapScene>(), 2);
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
            StateController.GameState = victory
                ? AutoBattleGameState.Victory
                : AutoBattleGameState.Defeat;

            Debug.Log(victory ? "승리!" : "패배…");
        }
    }
}