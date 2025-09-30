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
 
        protected PoolManager _poolManager;
        protected Poolable _poolable;
        protected CancellationTokenSource _cancellationSource;
        protected IAttacker _attacker;
        protected IVictim _victim;
        protected Vector2 _destination;
        protected DamageCalculator _damageCalculator;
        protected Action<IAttacker, IVictim ,DamageCalculator> OnApplyDamage;
    
        protected virtual void Awake()
        {
            _poolManager = PoolManager.Instance;
            _poolable = GetComponent<Poolable>();
        }

        public void SetData(IAttacker attacker, IVictim victim, Vector2 destination, DamageCalculator damageCalculator, Action<IAttacker, IVictim, DamageCalculator> onApplyDamage)
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