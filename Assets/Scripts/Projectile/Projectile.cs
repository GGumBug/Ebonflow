using CombatSystem;
using System;
using UnityEngine;

namespace ProjectileSystem
{
    public abstract class Projectile : MonoBehaviour, IUpdateObserver
    {
        [SerializeField] protected float speed = 20f;

        protected const float TARGET_Y_OFFSET = 0.5f;

        private bool isActive = false;
        private UpdateManager _updateManager;

        protected Vector2 _direction = Vector2.zero;
        protected PoolManager _poolManager;
        protected Poolable _poolable;
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
            _updateManager = UpdateManager.Instance;
        }

        public void SetData(IAttacker attacker, IVictim victim, Vector2 destination, DamageCalculator damageCalculator, bool isManaGain, Action<IAttacker, IVictim, DamageCalculator, bool> onApplyDamage)
        {
            _attacker = attacker;
            _victim = victim;
            _destination = destination;
            _damageCalculator = damageCalculator;
            OnApplyDamage = onApplyDamage;
        }

        public void ObservedUpdate()
        {
            if (!isActive)
                return;

            SetDestination();
            SetDirection();

            LookAtDirection2D();

            transform.position = Vector2.MoveTowards(
                                    transform.position,
                                    _destination,
                                    speed * Time.deltaTime
                                    );

            CheckComplete();
        }

        protected virtual void OnEnable()
        {
            isActive = true;
            _updateManager.RegisterObserver(this);
        }
    
        protected virtual void OnDisable()
        {
            _updateManager.UnRegisterObserver(this);
            Clear();
        }
    
        /// <summary>
        /// 발사체의 공통 상태를 초기화합니다.
        /// 자식 클래스에서 이 메서드를 오버라이드할 때 반드시 base.Clear()를 호출해야 합니다.
        /// </summary>
        protected virtual void Clear()
        {
            isActive = false;

            // ★ DOKill이 여전히 필요한 경우 (DOTween을 사용하지 않더라도 안전을 위해 유지 가능)
            // transform.DOKill(true);

            _direction = Vector2.zero;
            _destination = Vector2.zero;
            _attacker = null;
        }

        protected abstract void SetDestination();

        private void SetDirection()
        {
            _direction = (_destination - (Vector2)transform.position).normalized;
        }

        protected void LookAtDirection2D()
        {
            if (_direction != Vector2.zero)
            {
                float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        protected abstract void CheckComplete();

        protected void CompleteActionAndReturnToPool()
        {
            OnApplyDamage?.Invoke(_attacker, _victim, _damageCalculator, _isManaGain);

            _poolManager.Push(_poolable);

            transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        }
    }
}