using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class SceneBase : MonoBehaviour, ILoadableScene
{
    private void Awake()
    {
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
}
