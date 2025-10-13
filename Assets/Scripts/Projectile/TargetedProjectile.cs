using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace ProjectileSystem
{
    public class TargetedProjectile : Projectile
    {
        private Vector2 _targetPos;

        private void SetTargetPos()
        {
            _targetPos = (Vector2)_victim.Position;
        }

        public override void Launch()
        {
            SetTargetPos();

            Vector2 direction = (_targetPos - (Vector2)transform.position).normalized;

            if (direction != Vector2.zero)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                transform.rotation = Quaternion.Euler(0, 0, angle);
            }

            MoveAsync(_cancellationSource.Token).Forget();
        }

        protected override async UniTaskVoid MoveAsync(CancellationToken cancellationToken)
        {
            try
            {
                float duration = Vector2.Distance(transform.position, _targetPos) / speed;

                await transform.DOMove(_targetPos, duration)
                                .SetEase(Ease.Linear)
                                .ToUniTask(cancellationToken: cancellationToken);

                OnApplyDamage?.Invoke(_attacker, _victim, _damageCalculator, _isManaGain);
            }
            catch
            {
                Debug.Log("Projectile task was cancelled.");
            }
            finally
            {
                _poolManager.Push(_poolable);
            }
        }

        protected override void Clear()
        {
            base.Clear();

            _targetPos = Vector2.zero;
        }
    }
}