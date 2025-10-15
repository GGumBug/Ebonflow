using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using CombatSystem;
using System;

namespace ProjectileSystem
{
    public abstract class Projectile : MonoBehaviour
    {
        [SerializeField] protected float speed = 10f;

        protected const float TARGET_Y_OFFSET = 0.5f;

        protected Vector2 _direction = Vector2.zero;
        protected PoolManager _poolManager;
        protected Poolable _poolable;
        protected CancellationTokenSource _cancellationSource;
        protected IAttacker _attacker;
        protected IVictim _victim;
        protected Vector2 _destination;
        protected DamageCalculator _damageCalculator;
        protected bool _isManaGain;
        protected Action<IAttacker, IVictim, DamageCalculator, bool> OnApplyDamage;
    
        protected virtual void Awake()
        {
            _poolManager = PoolManager.Instance;
            _poolable = GetComponent<Poolable>();
        }

        public void SetData(IAttacker attacker, IVictim victim, Vector2 destination, DamageCalculator damageCalculator, bool isManaGain, Action<IAttacker, IVictim, DamageCalculator, bool> onApplyDamage)
        {
            _attacker = attacker;
            _victim = victim;
            _destination = destination;
            _damageCalculator = damageCalculator;
            OnApplyDamage = onApplyDamage;
        }
    
        protected virtual void OnEnable()
        {
            _cancellationSource = new CancellationTokenSource();
        }
    
        protected virtual void OnDisable()
        {
            // OnDisable에서 Clear를 호출하여 풀에 반환될 때 정리하도록 합니다.
            Clear();
        }
    
        /// <summary>
        /// 발사체의 공통 상태를 초기화합니다.
        /// 자식 클래스에서 이 메서드를 오버라이드할 때 반드시 base.Clear()를 호출해야 합니다.
        /// </summary>
        protected virtual void Clear()
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

        protected Vector2 ClampDirectionTo8Ways(Vector2 direction)
        {
            // 1. 현재 각도 계산 (라디안 -> 도)
            float currentAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Atan2는 -180도 ~ 180도 범위를 반환하지만, 0도 ~ 360도로 변환하여 계산하기 쉽게 만듭니다.
            if (currentAngle < 0)
            {
                currentAngle += 360f;
            }

            // 2. ★ 45도 단위로 반올림
            const float angleStep = 45f;

            // Mathf.Round()를 사용하여 가장 가까운 45도의 배수를 찾습니다.
            // 예: 22도 -> 0도, 23도 -> 45도, 80도 -> 90도
            float clampedAngle = Mathf.Round(currentAngle / angleStep) * angleStep;

            // 3. 반올림 후 360도를 초과하면 0도로 처리 (선택 사항)
            if (clampedAngle >= 360f)
            {
                clampedAngle -= 360f;
            }

            // 4. 새로운 방향 벡터 계산 (도 -> 라디안)
            float radian = clampedAngle * Mathf.Deg2Rad;

            // 새로운 정규화된 방향 벡터 반환
            return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian)).normalized;
        }

        protected void LookAtDirection2D()
        {
            if (_direction != Vector2.zero)
            {
                float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;

                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        protected abstract void InitializeLaunchDirection();

        /// <summary>
        /// 발사체를 발사합니다. 발사에 필요한 데이터는 이 메서드가 호출되기 전에
        /// 각 자식 클래스의 고유한 Set... 메서드를 통해 설정되어야 합니다.
        /// </summary>
        public abstract void Launch();
    
        /// <summary>
        /// 발사체의 이동 로직을 구현합니다.
        /// </summary>
        protected abstract UniTaskVoid MoveAsync(CancellationToken cancellationToken);
    }
}