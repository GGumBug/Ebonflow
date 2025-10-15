using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class SceneBase : MonoBehaviour, ILoadableScene
{
    public bool isDebugMode = false;

    private async void Awake()
    {
        bool isLoadedViaManager = SceneLoadManager.Instance.IsLoadingFlowActive;

        if (isDebugMode)
        {
            if (isLoadedViaManager)
            {
                Debug.LogWarning($"[SceneBase] '{gameObject.scene.name}' 씬이 정상 로딩 플로우를 통해 시작되었습니다. 디버그 모드를 무시합니다.");

                isDebugMode = false;
                SetSceneLoadCallbacks();
            }
            else
            {
                await DebugMode();
            }
        }
        else
        {
            SetSceneLoadCallbacks();
        }
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
