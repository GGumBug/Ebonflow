using System;
using UnityEngine;

namespace AutoBattle
{
    public class AutoBattleManager : Singleton<AutoBattleManager>
    {
        private RewardService _rewardService;

        public AutoBattleStateController StateController { get; private set; }

        public void Setup()
        {
            AutoBattleDataManager autoBattleDataManager = AutoBattleDataManager.Instance;
            _rewardService = new RewardService(autoBattleDataManager.AutoBattlePlayerDataContext);
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