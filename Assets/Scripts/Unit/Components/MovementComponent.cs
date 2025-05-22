using UnityEngine;
using DG.Tweening;
using System;

public class MovementComponent
{
    private float moveSpeed = 6f;
    private float stepDelay = 0.5f;

    private Transform _transform;
    private Tween _moveTween;

    public event Action CancelMovementAction;
    public event Action OnEndMove;

    public MovementComponent(Transform transform)
    {
        _transform = transform;
    }

    public void Move(Vector2Int destPos)
    {
        // 목표 위치를 Vector3로 변환 (z값은 현재 위치 유지)
        Vector3 destination = new Vector3(destPos.x, destPos.y, _transform.position.z);

        // 현재 위치와 목표 위치 사이의 거리를 계산하고, 이동 시간(duration)을 결정합니다.
        float distance = Vector2.Distance(_transform.position, new Vector2(destPos.x, destPos.y));
        float duration = distance / moveSpeed;

        // DOTween을 사용하여 선형 보간으로 이동시키고, 이동이 완료되면 SnapAndAdvance를 호출합니다.
        _moveTween = _transform.DOMove(destination, duration)
                    .SetEase(Ease.Linear)
                    .SetDelay(stepDelay)
                    .OnComplete(() => SnapAndAdvance(destPos));
    }

    private void SnapAndAdvance(Vector2Int destPos)
    {
        _transform.position = new Vector2(destPos.x, destPos.y);

        OnEndMove?.Invoke();
    }

    public void CancelMovement()
    {
        CancelMovementAction?.Invoke();

        _moveTween?.Kill();
        _moveTween = null;
    }
}
