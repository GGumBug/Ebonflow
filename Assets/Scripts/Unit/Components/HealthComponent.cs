using System;
using UnityEngine;

/// <summary>
/// UnitStats(HP의 source of truth)를 감싸 UI/상태머신용 이벤트를 제공하는 어댑터.
/// 체력 수치 변경 시 OnChanged, 0이 될 때 최초 1회 OnDied를 알립니다.
/// </summary>
public class HealthComponent
{
    private readonly UnitStats _stats;
    private bool _deadInvoked;

    /// <summary>(current, max) 체력 변화 브로드캐스트</summary>
    public event Action<int, int> OnChanged;

    /// <summary>처음으로 0 이하가 되었을 때</summary>
    public event Action OnDied;

    public int Current => Mathf.Clamp(_stats.CurrentHP, 0, Max);
    public int Max     => Mathf.Max(1, _stats.MaxHP);

    public HealthComponent(UnitStats stats)
    {
        _stats = stats ?? throw new ArgumentNullException(nameof(stats));

        OnChanged?.Invoke(Current, Max);
        _deadInvoked = Current <= 0;
    }

    /// <summary>기존 시그니처 유지(호환)</summary>
    public bool ApplyDamage(int dmg)
    {
        return ApplyDamageAndGetApplied(dmg, out _);
    }

    /// <summary>
    /// 피해를 적용하고 실제로 깎인 양을 out으로 반환합니다.
    /// </summary>
    public bool ApplyDamageAndGetApplied(int dmg, out int appliedDamage)
    {
        int before = _stats.CurrentHP;
        int incoming = Mathf.Max(0, dmg);
        int after = Mathf.Max(0, before - incoming);

        appliedDamage = before - after; // 실제로 깎인 양(쉴드/저항 고려 시 여기에 반영)
        _stats.TakeDamage(appliedDamage);

        if (_stats.CurrentHP <= 0)
        {
            OnDied?.Invoke();
            return true;
        }
        return false;
    }

    /// <summary>치유 적용 → 변경 이벤트 중계</summary>
    public void Heal(int amount)
    {
        int before = Current;
        _stats.Heal(Mathf.Max(0, amount));
        int after = Current;

        if (after != before)
            OnChanged?.Invoke(after, Max);

        // 치유로 부활 로직이 없다면 _deadInvoked는 그대로 유지
    }

    /// <summary>
    /// 레벨업/버프 등으로 MaxHP가 변한 직후 UI를 갱신해야 할 때 호출.
    /// (UnitStats.ChangeLevel 호출 이후 한 번 불러 주세요)
    /// </summary>
    public void Rebind()
    {
        OnChanged?.Invoke(Current, Max);
        if (Current <= 0 && !_deadInvoked)
        {
            _deadInvoked = true;
            OnDied?.Invoke();
        }
    }
}