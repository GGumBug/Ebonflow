using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class SceneBase : MonoBehaviour, ILoadableScene
{
    public bool isDebugMode = false;

    private void Awake()
    {
        if (isDebugMode)
            DebugMode();
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

    public abstract UniTask DebugMode();
}
