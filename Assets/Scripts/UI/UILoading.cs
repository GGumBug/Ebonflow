using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

public class UILoading : MonoBehaviour
{
    private float fadeDuration = 1f;
    private CanvasGroup canvasGroup;
    private TextMeshProUGUI txtLoadingPercent;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        txtLoadingPercent = GetComponentInChildren<TextMeshProUGUI>();

        SceneLoadManager.Instance.OnSceneLoadComplete += LoadingEndAction;
    }

    private async void OnEnable()
    {
        await LoadingStartAnimation();
    }

    private void OnDisable()
    {
        DOTween.Kill(canvasGroup); // 안전하게 Tween 제거
    }

    private void OnDestroy()
    {
        SceneLoadManager.Instance.OnSceneLoadComplete -= LoadingEndAction;
    }

    public async UniTask LoadingStartAnimation()
    {
        await canvasGroup.DOFade(1, fadeDuration);
        await SceneLoadManager.Instance.ActivateTargetScene();
    }

    public void UpdateProgress(float value)
    {
        txtLoadingPercent.text = $"{Mathf.FloorToInt(value)}%";
    }

    private async void LoadingEndAction()
    {
        await canvasGroup.DOFade(0, fadeDuration);
        await SceneManager.UnloadSceneAsync(Constants.LOADING_SCENE_NAME);
    }
}
