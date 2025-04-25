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
        _currentTarget = _currentTarget != null && _detector.IsTargetInRange(_currentTarget)
            ? _currentTarget
            : _detector.GetClosestEnemy();

        if (_currentTarget == null) 
        {
            OnAttackEnded?.Invoke();
            return;
        }

        OnAttackStarted?.Invoke();
        DOTween.Sequence()
        .AppendInterval(0.5f)
        .AppendCallback(
            () => {
                _autoBattleManager.Attack(_unit, _currentTarget);
                if (_currentTarget != null && _detector.IsTargetInRange(_currentTarget))
                {
                    TryAttack();
                }
                else
                {
                    OnAttackEnded?.Invoke();    
                }
                });
    }
}
