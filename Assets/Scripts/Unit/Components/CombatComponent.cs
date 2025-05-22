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
        _currentTarget = HasValidTarget
            ? _currentTarget
            : _detector.GetClosestEnemy();

        if (_currentTarget == null)
        {
            OnAttackEnded?.Invoke();
            return;
        }

        OnAttackStarted?.Invoke();

        _attackSequence = DOTween.Sequence()
            .AppendInterval(_unit.Stat.AttackDelay)
        .AppendCallback(() =>
        {
            bool targetDied = OnAttack(_unit, _currentTarget);

            OnAttackEnded?.Invoke();
        });
    }

    public void CancelAttack()
    {
        _attackSequence?.Kill();
        _attackSequence = null;
    }
}
