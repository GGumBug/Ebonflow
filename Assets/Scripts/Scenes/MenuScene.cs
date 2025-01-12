using Cysharp.Threading.Tasks;

public class MenuScene : SceneBase
{
    public override async UniTask LoadAssets()
    {
        await UniTask.Yield();
    }

    public override async UniTask InitializeData()
    {
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
}
