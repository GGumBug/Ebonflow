using AutoBattle;
using Cysharp.Threading.Tasks;
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

        await UniTask.Delay(2000);

        await SceneLoadManager.Instance.LoadSceneAsyncWithLoadingUI<MapScene>();
    }
}
