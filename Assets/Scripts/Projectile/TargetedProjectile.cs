using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace ProjectileSystem
{
    public class TargetedProjectile : Projectile
    {
        public override void Launch()
        {
            InitializeLaunchDirection();

            MoveAsync(_cancellationSource.Token).Forget();
        }

        protected override async UniTaskVoid MoveAsync(CancellationToken cancellationToken)
        {
            try
            {
                float duration = Vector2.Distance(transform.position, _destination) / speed;

                await transform.DOMove(_destination, duration)
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

            _destination = Vector2.zero;
        }

        protected override void InitializeLaunchDirection()
        {
            var offsetDest = new Vector2(_victim.Position.x, _victim.Position.y + TARGET_Y_OFFSET);
            _destination = offsetDest;
            _direction = (_destination - (Vector2)transform.position).normalized;

            _direction = ClampDirectionTo8Ways(_direction);

            LookAtDirection2D();
        }
    }
}