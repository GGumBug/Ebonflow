using AutoBattle;
using Cysharp.Threading.Tasks;
using UnityEngine;
using RoguelikeMap;

public class MenuScene : SceneBase
{
    [SerializeField] private UIStartMenu uiStartMenu;

    private AutoBattleDataManager _autoBattleDataManager;
    private MapSaveLoadManager _mapSaveLoadManager;

    public override async UniTask LoadAssets()
    {
        _autoBattleDataManager = AutoBattleDataManager.Instance;
        _mapSaveLoadManager = MapSaveLoadManager.Instance;
        await UniTask.Yield();
    }

    public override async UniTask InitializeData()
    {
        uiStartMenu.Setup(StartGame, ResetGameData);
        await UniTask.Yield();
    }

    public override async UniTask SetupScene()
    {
        await UniTask.Yield();
    }

    public override async UniTask FinalizeLoading()
    {
        await UniTask.Yield();
    }

    private async void StartGame()
    {
        if (_autoBattleDataManager.AutoBattleSceneDataContext.Stage.shouldResumeBattle)
            await SceneLoadManager.Instance.LoadSceneAsyncWithLoadingUI<AutoBattleScene>();
        else
            await SceneLoadManager.Instance.LoadSceneAsyncWithLoadingUI<MapScene>();
    }

    private void ResetGameData()
    {
        bool autobattleDataResult = _autoBattleDataManager.DeleteData();
        bool mapDataResult = _mapSaveLoadManager.DeleteMap();

        if (autobattleDataResult && mapDataResult)
            Debug.Log("데이터가 초기화 됐습니다.");
        else
            Debug.LogWarning("데이터가 초기화 실패.");
    }
}
