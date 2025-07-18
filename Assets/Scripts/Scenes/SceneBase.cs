using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class SceneBase : MonoBehaviour, ILoadableScene
{
    public bool isDebugMode = false;

    private async void Awake()
    {
        if (isDebugMode)
            await DebugMode();
        else
            SetSceneLoadCallbacks();
    }

    void SetSceneLoadCallbacks()
    {
        var SceneLoadCallBacks = new SceneLoadCallbacks(
            loadAssets: LoadAssets, 
            initializeData: InitializeData, 
            setupScene: SetupScene, 
            finalizeLoading: FinalizeLoading
            );

        SceneLoadManager.Instance.Callbacks = SceneLoadCallBacks;
    }

    public abstract UniTask LoadAssets();
    public abstract UniTask InitializeData();
    public abstract UniTask SetupScene();
    public abstract UniTask FinalizeLoading();

    public virtual async UniTask DebugMode()
    {
        await LoadAssets();

        await InitializeData();

        await SetupScene();

        await FinalizeLoading();
    }
}
