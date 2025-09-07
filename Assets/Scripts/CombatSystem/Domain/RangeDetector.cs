using System;
using System.Collections.Generic;
using UnityEngine;

namespace CombatSystem
{
    public class RangeDetector : MonoBehaviour
    {
        [Header("Detection Settings")]
        [Tooltip("월드 단위의 감지 반지름입니다.")]
        [SerializeField] private float detectionRadius = 5f;

        [Tooltip("감지할 오브젝트의 레이어 마스크입니다.")]
        [SerializeField] private LayerMask detectionLayer;

        [Header("Collider Settings")]
        [Tooltip("RangeDetector 전용 2D 콜라이더입니다.")]
        [SerializeField] private CircleCollider2D col;

        private LayerMask _unitLayerMask = 1 << 7; // Unit Mask
        private HashSet<IVictim> _inRangeEnemies;

        public event Func<int> OnRequestTeamId;
        public event Action OnEnemyListEmpty;

        private float _detectionRadius => col.radius * Mathf.Max(transform.localScale.x, transform.localScale.y);
        public bool HasEnemies() => _inRangeEnemies != null && _inRangeEnemies.Count > 0;

        public void Setup(int range)
        {
            detectionRadius = range;
            col.radius = range;
            _inRangeEnemies = new HashSet<IVictim>();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (((1 << collision.gameObject.layer) & _unitLayerMask) == 0)
                return;

            if (collision.TryGetComponent(out IVictim otherUnit))
            {
                TryRegisterEnemyUnit(otherUnit);
            }
        }

        public List<IVictim> FindEnemiesInRange()
        {
            var enemies = new List<IVictim>();

            var hits = Physics2D.OverlapCircleAll(transform.position, _detectionRadius, _unitLayerMask);
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<IVictim>(out var unit))
                {
                    TryRegisterEnemyUnit(unit);
                }
            }

            return enemies;
        }

        private void TryRegisterEnemyUnit(IVictim otherUnit)
        {
            if (otherUnit.TeamId != OnRequestTeamId.Invoke()
                && otherUnit.IsBattleActive
                && !_inRangeEnemies.Contains(otherUnit))
            {
                RegisterEnemyUnit(otherUnit);
            }
        }

        public IVictim GetClosestEnemy()
        {
            IVictim closestEnemy = null;
            float minDistance = float.MaxValue;

            foreach (IVictim enemy in _inRangeEnemies)
            {
                float distance = Vector2.Distance(transform.position, enemy.Position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestEnemy = enemy;
                }
            }

            return closestEnemy;
        }

        public bool IsTargetInRange(IVictim target)
        {
            return target != null && _inRangeEnemies.Contains(target);
        }

        private void RegisterEnemyUnit(IVictim u)
        {
            _inRangeEnemies.Add(u);
            u.OnDied += HandleUnitDied;
        }

        private void HandleUnitDied(IVictim u)
        {
            u.OnDied -= HandleUnitDied;
            if (_inRangeEnemies.Remove(u) && _inRangeEnemies.Count == 0)
                OnEnemyListEmpty?.Invoke();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }
    }
}