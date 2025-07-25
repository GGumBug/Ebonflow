using System;
using DG.Tweening;

public class CombatComponent
{
    public event Action OnAttackStarted;
    public event Action OnAttackEnded;

    private RangeDetector _detector;
    private event Func<Unit, Unit, bool> OnAttack;
    private Unit _unit;
    private Unit _currentTarget;
    private Sequence _attackSequence;

    private bool HasValidTarget =>
    _currentTarget != null
    && !_currentTarget.IsDead
    && _detector.IsTargetInRange(_currentTarget);

    public CombatComponent(Unit host, RangeDetector detector, Func<Unit, Unit, bool> onAttack)
    {
        _unit = host;
        _detector = detector;
        OnAttack += onAttack;
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
        var attacker = _unit;
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

                bool targetDied = OnAttack(attacker, target);
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
