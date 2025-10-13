using System;
using CombatSystem;
using DG.Tweening;
using UnityEngine;

public class CombatComponent : IDisposable
{
    private IRangeDetector _detector;
    private IAttacker _host;
    private Sequence _attackSequence;
    private Func<IAttacker, IRangeDetector, bool, bool> OnTrigger;

    public event Action OnAttackStarted;
    public event Action OnAttackEnded;
    public event Action<int> ResetToMana;
    public event Func<bool> CheckManaFull;

    public CombatComponent(Unit host, RangeDetector detector, Func<IAttacker, IRangeDetector, bool, bool> onTrigger, ManaComponent manaComponent)
    {
        _host = host;
        _detector = detector;
        OnTrigger += onTrigger;
        CheckManaFull += manaComponent.IsFull;
        ResetToMana += manaComponent.ResetTo;
    }

    public bool CanAttack()
    {
        if (_detector.HasEnemies)
            return true;

        return false;
    }

    public void TryAttack()
    {
        // 기존 시퀀트가 남아 있으면 즉시 취소
        if (_attackSequence != null && _attackSequence.IsActive())
            _attackSequence.Kill();

        // 클로저 안전성 확보를 위해 로컬 변수에 복사
        var attacker = _host;

        if (CheckManaFull.Invoke())
        {
            ResetToMana.Invoke(0);
            Debug.Log("스킬 발동!");
        }
        else
        {
            bool result = OnTrigger(attacker, _detector, true); // 디버그용 스킬 0번
            if (!result)
            {
                OnAttackEnded?.Invoke();
                return;
            }
        }

        OnAttackStarted?.Invoke();

        _attackSequence = DOTween.Sequence()
            .AppendInterval(attacker.Stat.AttackDelay)
            .AppendCallback(() =>
            {
                OnAttackEnded?.Invoke();
            })
            .SetAutoKill(true);
    }

    public void CancelAttack()
    {
        _attackSequence?.Kill();
        _attackSequence = null;
    }

    public void Dispose()
    {
        CancelAttack();
        _detector = null;
        _host = null;
    }
}
