using System;
using CombatSystem;
using DG.Tweening;
using UnityEngine;

public class CombatComponent
{
    private RangeDetector _detector;
    private event Func<IAttacker, IVictim, bool> OnAttack;
    private IAttacker _host;
    private IVictim _currentTarget;
    private Sequence _attackSequence;

    public event Action OnAttackStarted;
    public event Action OnAttackEnded;
    public event Action<int> ResetToMana;
    public event Func<bool> CheckManaFull;

    private bool HasValidTarget =>
    _currentTarget != null
    && !_currentTarget.IsDead
    && _detector.IsTargetInRange(_currentTarget);

    public CombatComponent(Unit host, RangeDetector detector, Func<IAttacker, IVictim, bool> onAttack, ManaComponent manaComponent)
    {
        _host = host;
        _detector = detector;
        OnAttack += onAttack;
        CheckManaFull += manaComponent.IsFull;
        ResetToMana += manaComponent.ResetTo;
    }

    public bool CanAttack()
    {
        if (_detector.HasEnemies())
            return true;

        return false;
    }

    public void TryAttack()
    {
        // 기존 시퀀트가 남아 있으면 즉시 취소
        if (_attackSequence != null && _attackSequence.IsActive())
            _attackSequence.Kill();

        // 유효 타겟 확보
        _currentTarget = HasValidTarget
            ? _currentTarget
            : _detector.GetClosestEnemy();

        if (_currentTarget == null)
        {
            OnAttackEnded?.Invoke();
            return;
        }

        OnAttackStarted?.Invoke();

        // 클로저 안전성 확보를 위해 로컬 변수에 복사
        var attacker = _host;
        var target = _currentTarget;

        _attackSequence = DOTween.Sequence()
            .AppendInterval(attacker.Stat.AttackDelay)
            .AppendCallback(() =>
            {
                // 콜백 직전에 항상 null/죽음 검사
                if (target == null || target.IsDead)
                {
                    OnAttackEnded?.Invoke();
                    return;
                }

                if (CheckManaFull.Invoke())
                {
                    ResetToMana.Invoke(0);
                    Debug.Log("스킬 발동!");
                }
                else
                {
                    bool targetDied = OnAttack(attacker, target);
                }

                OnAttackEnded?.Invoke();
            })
            .SetAutoKill(true);  // 시퀀스 완료 후 자동 해제
    }

    public void CancelAttack()
    {
        _attackSequence?.Kill();
        _attackSequence = null;
    }
}
