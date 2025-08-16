using UnityEngine;

/// <summary>
/// 유닛의 Health/Mana를 UI 슬라이더(StatBarView)와 바인딩합니다.
/// 부모인 UIUnitStatBars 아래에 HP/MP StatBarView 두 개가 존재한다고 가정합니다.
/// </summary>
[DisallowMultipleComponent]
public class UIUnitStatBars : MonoBehaviour
{
    [Header("Child Bars")]
    [SerializeField] private StatBarView _hpBar;
    [SerializeField] private StatBarView _manaBar;

    private HealthComponent _health;
    private ManaComponent _mana;

    /// <summary>Health/Mana 컴포넌트를 바에 연결합니다.</summary>
    public void Bind(HealthComponent health, ManaComponent mana)
    {
        Unbind();

        _health = health;
        _mana   = mana;

        if (_health != null && _hpBar != null)
        {
            _hpBar.Setup(_health.Current, _health.Max);
            _health.OnChanged += OnHpChanged;
        }

        if (_mana != null && _manaBar != null)
        {
            _manaBar.Setup(_mana.Current, _mana.Max);
            _mana.OnChanged += OnManaChanged;
        }
    }

    /// <summary>연결 해제(이벤트 언바인딩)</summary>
    public void Unbind()
    {
        if (_health != null) _health.OnChanged -= OnHpChanged;
        if (_mana   != null) _mana.OnChanged   -= OnManaChanged;
        _health = null;
        _mana   = null;
    }

    private void OnDestroy() => Unbind();

    private void OnHpChanged(int cur, int max)
    {
        if (_hpBar != null) _hpBar.UpdateValue(cur, max);
    }

    private void OnManaChanged(int cur, int max)
    {
        if (_manaBar != null) _manaBar.UpdateValue(cur, max);
    }
}