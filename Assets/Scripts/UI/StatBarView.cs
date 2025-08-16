using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[DisallowMultipleComponent]
public class StatBarView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider _slider;

    [Header("Smoothing")]
    [SerializeField] private float _tweenDuration = 0.3f; // 보간 시간

    private Tween _tween;
    private int _max = 1;

    public void Setup(int current, int max)
    {
        _max = Mathf.Max(1, max);
        if (_slider != null)
        {
            _slider.minValue = 0;
            _slider.maxValue = _max;
            _slider.value    = Mathf.Clamp(current, 0, _max);
        }
    }

    public void UpdateValue(int current, int max)
    {
        _max = Mathf.Max(1, max);
        if (_slider != null)
        {
            if (_slider.maxValue != _max)
                _slider.maxValue = _max;

            float target = Mathf.Clamp(current, 0, _max);

            // 기존 트윈 중지
            _tween?.Kill();

            // DOTween 으로 트윈 시작
            _tween = _slider.DOValue(target, _tweenDuration)
                            .SetEase(Ease.OutQuad); // 부드러운 감속
        }
    }

    /// <summary>즉시 값 갱신 (트윈 없이)</summary>
    public void SnapToValue(int current, int max)
    {
        _tween?.Kill();
        _max = Mathf.Max(1, max);
        if (_slider != null)
        {
            _slider.maxValue = _max;
            _slider.value = Mathf.Clamp(current, 0, _max);
        }
    }

    private void OnDisable()
    {
        // 트윈 정리
        _tween?.Kill();
    }
}