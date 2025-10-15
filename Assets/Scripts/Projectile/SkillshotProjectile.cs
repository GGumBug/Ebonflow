using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using DG.Tweening;

namespace ProjectileSystem
{
    public class SkillshotProjectile : Projectile
    {
        [SerializeField] private LayerMask targetLayerMask;

        void OnTriggerEnter2D(Collider2D collision)
        {
            if ((targetLayerMask.value & (1 << collision.gameObject.layer)) > 0)
            {
                Unit triggeredUnit = null;
                if (collision.gameObject.TryGetComponent<Unit>(out triggeredUnit))
                {
                    if (_attacker.TeamId != triggeredUnit.TeamId)
                    {
                        OnApplyDamage?.Invoke(_attacker, triggeredUnit, _damageCalculator, _isManaGain);

                        _poolManager.Push(_poolable);
                    }
                }
            }
        }

        public override void Launch()
        {
            InitializeLaunchDirection();

            MoveAsync(_cancellationSource.Token).Forget();
        }

        protected async override UniTaskVoid MoveAsync(CancellationToken cancellationToken)
        {
            try
            {
                float duration = Vector2.Distance(transform.position, _destination) / speed;

                await transform.DOMove(_destination, duration)
                                .SetEase(Ease.Linear)
                                .ToUniTask(cancellationToken: cancellationToken);
            }
            catch
            {
                Debug.Log("Projectile task was cancelled.");
            }
            finally
            {
                if (gameObject.activeSelf)
                {
                    _poolManager.Push(_poolable);    
                }
            }
        }

        protected override void InitializeLaunchDirection()
        {
            var offsetDest = new Vector2(_destination.x, _destination.y + TARGET_Y_OFFSET);
            _destination = offsetDest;

            var originDirection = (offsetDest - (Vector2)_attacker.Position).normalized;
            _direction = ClampDirectionTo8Ways(originDirection);

            LookAtDirection2D();
        }
    }
}