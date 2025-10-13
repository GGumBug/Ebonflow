using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using DG.Tweening;

namespace ProjectileSystem
{
    public class SkillshotProjectile : Projectile
    {
        [SerializeField] private LayerMask targetLayerMask;

        private Vector2 _direction;

        void SetDirection()
        {
            _direction = ((Vector2)_attacker.Position - _destination).normalized;
        }

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
            SetDirection();

            if (_direction != Vector2.zero)
            {
                float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;

                transform.rotation = Quaternion.Euler(0, 0, angle);
            }

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
    }
}