using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UILoading : MonoBehaviour
{
    private float fadeDuration = 1f;
    private CanvasGroup canvasGroup;
    private TextMeshProUGUI txtLoadingPercent;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        txtLoadingPercent = GetComponentInChildren<TextMeshProUGUI>();
        SceneLoadManager.Instance.OnSceneLoadComplete += () => LoadingEndAction();
    }

    private async void OnEnable()
    {
        await LoadingStartAnimation();
    }

    public async UniTask LoadingStartAnimation()
    {
        await canvasGroup.DOFade(1, fadeDuration).OnComplete(async () => await SceneLoadManager.Instance.ActivateTargetScene());
    }

    public void UpdateProgress(float value)
    {
        txtLoadingPercent.text = value + "%";
    }

    private async void LoadingEndAction()
    {
        await canvasGroup.DOFade(0, fadeDuration).OnComplete(async () => await SceneManager.UnloadSceneAsync(Constants.LOADING_SCENE_NAME));
    }
}
