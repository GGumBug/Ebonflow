using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadManager : Singleton<SceneLoadManager>, IDonDestroy
{
    private const int PROGRESS_STEP_DELAY = 100;
    private const string LOADING_SCENE_NAME = "Loading";
    private const float LOAD_SCENE_PROGRESS_THRESHOLD = 0.89f;
    private readonly float[] STEP_PERCENTS = { 30f, 30f, 20f, 20f };

    private string currentSceneName;
    private string targetSceneName;
    private float currentStep = 0f;
    private float currentPercent = 0f;
    private float increasePercentage = 1f;
    private CancellationTokenSource cancel;
    private CancellationTokenSource linked;

    public Action previousSceneLoadProgressAction;
    public Action<float> updateProgressAction;
    public Func<UniTask> loadAssetsFunc;
    public Func<UniTask> initializeDataFunc;
    public Func<UniTask> setupSceneFunc;
    public Func<UniTask> finalizeLoadingFunc;
    public Func<Action, UniTask> loadingEndAnimationAction;

    public bool IsLoadAssetsAssigned => loadAssetsFunc != null;

    public async UniTask LoadSceneAsync<T>() where T : ILoadableScene
    {
        previousSceneLoadProgressAction?.Invoke();

        currentSceneName = SceneManager.GetActiveScene().name;
        targetSceneName = typeof(T).Name;
        // 타겟 씬 로딩
        await ActivateScene(targetSceneName);

        // 로딩 작업이 할당될 때까지 대기
        await UniTask.WaitUntil(() => IsLoadAssetsAssigned);

        // 기존 씬 언로드
        await SceneManager.UnloadSceneAsync(currentSceneName);

        // 각 단계의 로딩 작업 처리
        await LoadAllSteps();

        Clear();
    }

    public async UniTask ActivateLoadingSceneAsync<T>() where T : ILoadableScene
    {
        previousSceneLoadProgressAction?.Invoke();

        currentSceneName = SceneManager.GetActiveScene().name;
        targetSceneName = typeof(T).Name;

        // 로딩 씬 활성화
        await ActivateScene(LOADING_SCENE_NAME);
    }

    public async UniTask ActivateTargetScene()
    {
        // 타겟 씬 로딩
        await ActivateScene(targetSceneName);

        // 로딩 작업이 할당될 때까지 대기
        await UniTask.WaitUntil(() => IsLoadAssetsAssigned);

        var targetScene = SceneManager.GetSceneByName(targetSceneName);

        await UniTask.WaitUntil(() => targetScene.isLoaded);

        SceneManager.SetActiveScene(targetScene);

        await SceneManager.UnloadSceneAsync(currentSceneName);

        // 각 단계의 로딩 작업 처리
        await LoadAllSteps();

        // 로딩 완료 및 애니메이션 처리
        await FinishLoading();
    }

    private async UniTask ActivateScene(string sceneName)
    {
        var asyncOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        asyncOperation.allowSceneActivation = false;

        // 씬 로드가 거의 완료될 때까지 대기
        await UniTask.WaitUntil(() => asyncOperation.progress >= LOAD_SCENE_PROGRESS_THRESHOLD);

        // 씬 활성화
        asyncOperation.allowSceneActivation = true;
    }

    private async UniTask LoadAllSteps()
    {
        var loadSteps = new[]
        {
            new { Func = initializeDataFunc, StepPercent = STEP_PERCENTS[0] },
            new { Func = loadAssetsFunc, StepPercent = STEP_PERCENTS[1] },
            new { Func = setupSceneFunc, StepPercent = STEP_PERCENTS[2] },
            new { Func = finalizeLoadingFunc, StepPercent = STEP_PERCENTS[3] }
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

            await UniTask.Delay(PROGRESS_STEP_DELAY, cancellationToken: linked.Token); // 진행 간격만큼 대기
        }
    }

    private async UniTask FinishLoading()
    {
        // 로딩 완료 애니메이션 및 로딩 씬 언로드
        await loadingEndAnimationAction.Invoke(async () =>
        {
            await SceneManager.UnloadSceneAsync(LOADING_SCENE_NAME);
        });

        Clear();
    }

    void Clear()
    {
        currentSceneName = null;
        targetSceneName = null;
        currentPercent = 0f;
        updateProgressAction = null;
        loadAssetsFunc = null;
        initializeDataFunc = null;
        setupSceneFunc = null;
        finalizeLoadingFunc = null;
        loadingEndAnimationAction = null;

        cancel.Cancel();
        cancel.Dispose();
    }
}
