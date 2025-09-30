using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using CombatSystem;
using SkillSystem;

namespace ProjectileSystem
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 10f;

        private PoolManager _poolManager;
        private Poolable _poolable;
        private Vector2 _targetPosition;
        private CancellationTokenSource _cancellationSource;
        private IAttacker _attacker;
        private IVictim _victim;
        private DamageCalculator _damageCalculator;
        public Action<IAttacker, IVictim ,DamageCalculator> OnApplyDamage;

        void Awake()
        {
            _poolManager = PoolManager.Instance;
            _poolable = GetComponent<Poolable>();
        }

        public void SetProjectile(IAttacker attacker, IVictim victim, DamageCalculator damageCalculator, Action<IAttacker, IVictim, DamageCalculator> onApplyDamage)
        {
            _attacker = attacker;
            _victim = victim;
            _damageCalculator = damageCalculator;
            OnApplyDamage = onApplyDamage;
        }

        private void OnEnable()
        {
            _cancellationSource = new CancellationTokenSource();   
        }

        private void OnDisable()
        {
            Clear();
        }

        public void Launch(Vector2 targetPos)
        {
            Vector2 direction = (targetPos - (Vector2)transform.position).normalized;

            if (direction != Vector2.zero)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                transform.rotation = Quaternion.Euler(0, 0, angle);
            }

            MoveAsync(targetPos, _cancellationSource.Token).Forget();
        }

        private async UniTaskVoid MoveAsync(Vector2 targetPos, CancellationToken cancellationToken)
        {
            try
            {
                float duration = Vector2.Distance(transform.position, targetPos) / speed;

                await transform.DOMove(targetPos, duration)
                                .SetEase(Ease.Linear)
                                .ToUniTask(cancellationToken: cancellationToken);

                // 이동 완료후 로직
                // 데미지
            }
            catch
            {
                Debug.Log("Projectile task was cancelled.");
            }
            finally
            {
                OnApplyDamage?.Invoke(_attacker, _victim, _damageCalculator);
                _poolManager.Push(_poolable);
            }
        }

        private void Clear()
        {
            _cancellationSource?.Cancel();
            _cancellationSource?.Dispose();
            _cancellationSource = null;

            // 참조 타입들을 null로 초기화하여 이전 상태가 남지 않도록 합니다.
            _attacker = null;
            _victim = null; 
            _damageCalculator = null;
            OnApplyDamage = null;

            // 트랜스폼 정보 초기화 (선택적이지만 좋은 습관입니다)
            transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        }
    }
}