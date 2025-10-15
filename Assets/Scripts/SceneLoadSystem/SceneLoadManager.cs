using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public class SceneLoadManager : Singleton<SceneLoadManager>, IDonDestroy
{
    private const int PROGRESS_STEP_DELAY = 100;
    private const float LOAD_SCENE_PROGRESS_THRESHOLD = 0.89f;
    private readonly float[] STEP_PERCENTS = { 30f, 30f, 20f, 20f };

    private string currentSceneName;
    private string targetSceneName;
    private float currentStep = 0f;
    private float currentPercent = 0f;
    private float increasePercentage = 1f;
    private CancellationTokenSource cancel;
    private CancellationTokenSource linked;
    private Action<float> updateProgressAction;

    private bool HasAssignedLoadingTasks => Callbacks != null;

    public bool IsLoadingFlowActive { get; private set; }
    public SceneLoadCallbacks Callbacks { get; set; } = null;
    public Action PreviousSceneLoadProgressAction { get; set; }
    public Action OnSceneLoadComplete { get; set; }

    private void Awake()
    {
        PreviousSceneLoadProgressAction += () => { Destroy(Camera.main.GetComponent<AudioListener>()); };
        PreviousSceneLoadProgressAction += () => { DisableEventSystemAndModules(); };
    }

    public void DisableEventSystemAndModules()
    {
        var eventSystem = FindFirstObjectByType<EventSystem>();
        if (!eventSystem)
            return;

        eventSystem.GetComponent<InputSystemUIInputModule>().enabled = false;
        eventSystem.enabled = false;
    }

    public async UniTask LoadSceneAsync<T>(bool isLoadingEnabled = true)
    {
        IsLoadingFlowActive = true;

        PreviousSceneLoadProgressAction?.Invoke();

        currentSceneName = SceneManager.GetActiveScene().name;
        targetSceneName = typeof(T).Name;

        await LoadAndActivateSceneAsync(targetSceneName);

        await UniTask.WaitUntil(() => HasAssignedLoadingTasks);

        await SceneManager.UnloadSceneAsync(currentSceneName);

        await LoadAllSteps();

        Clear();
    }

    public async UniTask LoadSceneAsyncWithLoadingUI<T>() where T : ILoadableScene
    {
        IsLoadingFlowActive = true;

        PreviousSceneLoadProgressAction?.Invoke();

        currentSceneName = SceneManager.GetActiveScene().name;
        targetSceneName = typeof(T).Name;

        await LoadAndActivateSceneAsync(Constants.LOADING_SCENE_NAME);
    }

    private async UniTask LoadAndActivateSceneAsync(string sceneName)
    {
        var asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        asyncOperation.allowSceneActivation = false;

        await UniTask.WaitUntil(() => asyncOperation.progress >= LOAD_SCENE_PROGRESS_THRESHOLD);

        asyncOperation.allowSceneActivation = true;
    }

    public async UniTask ActivateTargetScene()
    {
        await LoadAndActivateSceneAsync(targetSceneName);

        await UniTask.WaitUntil(() => HasAssignedLoadingTasks);

        var targetScene = SceneManager.GetSceneByName(targetSceneName);

        await UniTask.WaitUntil(() => targetScene.isLoaded);

        SceneManager.SetActiveScene(targetScene);

        await SceneManager.UnloadSceneAsync(currentSceneName);

        await LoadAllSteps();

        FinishLoading();
    }

    private async UniTask LoadAllSteps()
    {
        var loadSteps = new[]
        {
            new { Func = Callbacks.LoadAssets, StepPercent = STEP_PERCENTS[0] },
            new { Func = Callbacks.InitializeData, StepPercent = STEP_PERCENTS[1] },
            new { Func = Callbacks.SetupScene, StepPercent = STEP_PERCENTS[2] },
            new { Func = Callbacks.FinalizeLoading, StepPercent = STEP_PERCENTS[3] }
        };

        cancel = new();
        linked = CancellationTokenSource.CreateLinkedTokenSource(cancel.Token, this.GetCancellationTokenOnDestroy());

        foreach (var step in loadSteps)
            await LoadStep(step.Func, step.StepPercent);
    }

    private async UniTask LoadStep(Func<UniTask> taskFunc, float maxPercentForStep)
    {
        float targetPercent = currentPercent + maxPercentForStep;
        var progressTask = UpdateProgressDynamic(targetPercent);

        await taskFunc.Invoke();

        if (currentPercent < maxPercentForStep)
            await UpdateProgressDynamic(targetPercent);

        currentStep++;
        currentPercent = targetPercent;
        updateProgressAction?.Invoke(currentPercent);
    }

    private async UniTask UpdateProgressDynamic(float targetPercent)
    {
        while (currentPercent < targetPercent)
        {
            currentPercent += increasePercentage;
            updateProgressAction?.Invoke(currentPercent);

            await UniTask.Delay(PROGRESS_STEP_DELAY, cancellationToken: linked.Token);
        }
    }

    private void FinishLoading()
    {
        OnSceneLoadComplete?.Invoke();

        Clear();
    }

    private void Clear()
    {
        currentSceneName = null;
        targetSceneName = null;
        currentPercent = 0f;
        updateProgressAction = null;
        Callbacks = null;
        IsLoadingFlowActive = false;

        cancel.Cancel();
        cancel.Dispose();
    }
}
