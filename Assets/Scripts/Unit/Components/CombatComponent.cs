using CombatSystem;
using DG.Tweening;
using System;

public class CombatComponent : IDisposable
{
    private bool _isAttacking = false;
    private IRangeDetector _detector;
    private IAttacker _host;
    private Sequence _attackSequence;
    private Func<bool, IAttacker, IRangeDetector, bool, Action<IAttacker, Action>, bool> OnTrigger;

    public event Action OnAttackStarted;
    public event Action OnAttackEnded;
    public event Action<int> ResetToMana;
    public event Func<bool> CheckManaFull;

    public CombatComponent(Unit host, RangeDetector detector, Func<bool, IAttacker, IRangeDetector, bool, Action<IAttacker, Action>, bool> onTrigger, ManaComponent manaComponent)
    {
        _host = host;
        _detector = detector;
        OnTrigger += onTrigger;
        CheckManaFull += manaComponent.IsFull;
        ResetToMana += manaComponent.ResetTo;
    }

    public bool TryAttack(out bool isActiveSkill)
    {
        if (_isAttacking)
        {
            isActiveSkill = false;
            return false;
        }

        // 기존 시퀀트가 남아 있으면 즉시 취소
        if (_attackSequence != null && _attackSequence.IsActive())
            _attackSequence.Kill();

        // 클로저 안전성 확보를 위해 로컬 변수에 복사
        var attacker = _host;

        if (CheckManaFull.Invoke())
        {
            ResetToMana.Invoke(0);
            bool result = OnTrigger(true, attacker, _detector, false, BeginAttackSequence);
            isActiveSkill = true;
            if (!result)
            {
                OnAttackEnded?.Invoke();
                return false;
            }
        }
        else
        {
            bool result = OnTrigger(false, attacker, _detector, true, BeginAttackSequence);
            isActiveSkill = false;
            if (!result)
            {
                OnAttackEnded?.Invoke();
                return false;
            }
        }

        return true;
    }

    private void BeginAttackSequence(IAttacker attacker, Action shootAction)
    {
        OnAttackStarted?.Invoke();

        _attackSequence = DOTween.Sequence()
            .AppendInterval(attacker.Stat.AttackFrameDelay)
            .AppendCallback(() =>
            {
                shootAction?.Invoke();
            })
            .AppendInterval(attacker.Stat.AttackDelay - attacker.Stat.AttackFrameDelay)
            .AppendCallback(() =>
            {
                OnAttackEnded?.Invoke();
                _isAttacking = false;
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
