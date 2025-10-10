using UnityEngine;

public class UnitAnimationController
{
    private readonly int ANIM_HASH_DIRECTION = Animator.StringToHash("Direction");
    private readonly int ANIM_HASH_IS_MOVING = Animator.StringToHash("IsMoving");
    private readonly int ANIM_HASH_ATTACK_TRIGGER = Animator.StringToHash("AttackTrigger");
    private readonly int ANIM_HASH_SKILL_TRIGGER = Animator.StringToHash("SkillTrigger");
    private readonly int ANIM_HASH_IS_DEAD = Animator.StringToHash("IsDead");

    private Animator _animator;
    private int _currentDirection = 0;

    public UnitAnimationController(Animator animator)
    {
        if (animator == null)
            Debug.LogWarning("Animator가 null입니다.");

        _animator = animator;
    }

    public void SetDirection(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.01f)
        {
            return;
        }

        int directionIndex = GetDirectionIndexOptimized(dir);

        _animator.SetFloat(ANIM_HASH_DIRECTION, directionIndex);
    }

    public void StopWalk()
    {
        _animator.SetBool(ANIM_HASH_IS_MOVING, false);

        _animator.SetFloat(ANIM_HASH_DIRECTION, _currentDirection);
    }

    public void SetWalk()
    {
        _animator.SetBool(ANIM_HASH_IS_MOVING, true);

        _animator.SetFloat(ANIM_HASH_DIRECTION, _currentDirection);
    }

    /// <summary>
    /// 방향 벡터(dir)를 0부터 7까지의 8방향 인덱스로 변환합니다.
    /// 인덱스 0 = East, 1 = NE, 2 = North, ... (시계 반대 방향)
    /// </summary>
    private int GetDirectionIndexOptimized(Vector2 dir)
    {
        // 영벡터는 방향을 결정할 수 없으므로, 마지막 방향을 유지하거나 0(기본값)을 반환합니다.
        if (dir.sqrMagnitude < 0.01f)
        {
            return _currentDirection;
        }

        // 1. 각도를 0~360 범위로 가져오기 (동쪽(E)을 0도로 설정)
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        // 2. 인덱스 매핑을 위한 각도 조정: 
        // Unity의 0도 (East)를 기준으로 22.5도 이동하여 분할 경계를 중앙에 위치시킵니다.
        // (0도~45도 사이가 하나의 방향 인덱스에 매핑되도록 조정)
        float indexFloat = (angle + 22.5f) / 45f;

        // 3. 인덱스 계산 (0~7)
        // 8 이상이면 0으로 순환 (wrap around)
        int rawIndex = (int)indexFloat % 8;

        _currentDirection = rawIndex;
        return rawIndex;
    }

    public void TriggerAttack()
    {
        _animator.SetTrigger(ANIM_HASH_ATTACK_TRIGGER);
    }

    public void TriggerSkill()
    {
        _animator.SetTrigger(ANIM_HASH_SKILL_TRIGGER);
    }

    public void SetDead(bool isDead)
    {
        _animator.SetBool(ANIM_HASH_IS_DEAD, isDead);
    }
}
