using System;
using DG.Tweening;

public class CombatComponent
{
    public event Action OnAttackStarted;
    public event Action OnAttackEnded;

    private RangeDetector _detector;
    private AutoBattleManager _autoBattleManager;
    private Unit _unit;
    private Unit _currentTarget;
    private Sequence _attackSequence;

    private bool HasValidTarget =>
    _currentTarget != null
    && !_currentTarget.IsDead
    && _detector.IsTargetInRange(_currentTarget);

    public CombatComponent(Unit host, RangeDetector detector)
    {
        _unit = host;
        _detector = detector;
        _autoBattleManager = AutoBattleManager.Instance;
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
            bool targetDied = _autoBattleManager.Attack(_unit, _currentTarget);

            if (targetDied)
            {
                OnAttackEnded?.Invoke();
                return;
            }

            if (_detector.IsTargetInRange(_currentTarget))
                TryAttack();
            else
                OnAttackEnded?.Invoke();
        });
    }

    public void CancelAttack()
    {
        _attackSequence?.Kill();
        _attackSequence = null;
    }
}
