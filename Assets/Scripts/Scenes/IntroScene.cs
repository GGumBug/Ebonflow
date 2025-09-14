using AutoBattle;
using Cysharp.Threading.Tasks;
using RoguelikeMap;
using UnityEngine;

public class IntroScene : MonoBehaviour
{
    private async void Awake()
    {
        await TestIntro();
    }

    private async UniTask TestIntro()
    {
        AutoBattleDataManager autoBattleDataManager = AutoBattleDataManager.Instance;
        autoBattleDataManager.Setup();

        var _mapGenerationSettings = await AddressableManager.Instance.Load<MapGenerationSettings>(AddressableKey.MapGenerationSettings);
        MapSaveLoadManager.Instance.Init(_mapGenerationSettings);

        await UniTask.Delay(2000);

        await SceneLoadManager.Instance.LoadSceneAsyncWithLoadingUI<MenuScene>();
    }
}
