using System;
using System.Collections.Generic;
using UnityEngine;

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
    private HashSet<Unit> _enemyUnits;

    public event Func<TeamType> OnRequestTeamType;
    public event Action OnEnemyListEmpty;

    private float _detectionRadius => col.radius * Mathf.Max(transform.localScale.x, transform.localScale.y);
    public bool HasEnemies() => _enemyUnits != null && _enemyUnits.Count > 0;

    public void Setup(int range)
    {
        detectionRadius = range;
        col.radius = range;
        _enemyUnits = new HashSet<Unit>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & _unitLayerMask) == 0)
            return;

        if (collision.TryGetComponent(out Unit otherUnit))
        {
            TryRegisterEnemyUnit(otherUnit);
        }
    }

    public List<Unit> FindEnemiesInRange()
    {
        var enemies = new List<Unit>();

        var hits = Physics2D.OverlapCircleAll(transform.position, _detectionRadius, _unitLayerMask);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<Unit>(out var unit))
            {
                TryRegisterEnemyUnit(unit);
            }
        }

        return enemies;
    }

    private void TryRegisterEnemyUnit(Unit otherUnit)
    {
        if (otherUnit.GetTeam() != OnRequestTeamType.Invoke()
            && otherUnit.IsBattleActive
            && !_enemyUnits.Contains(otherUnit))
        {
            RegisterEnemyUnit(otherUnit);
        }
    }

    public Unit GetClosestEnemy()
    {
        Unit closestEnemy = null;
        float minDistance = float.MaxValue;

        foreach (Unit enemy in _enemyUnits)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestEnemy = enemy;
            }
        }

        return closestEnemy;
    }

    public bool IsTargetInRange(Unit target)
    {
        return target != null && _enemyUnits.Contains(target);
    }

    private void RegisterEnemyUnit(Unit u)
    {
        _enemyUnits.Add(u);
        u.OnDied += HandleUnitDied;
    }

    private void HandleUnitDied(Unit u)
    {
        u.OnDied -= HandleUnitDied;
        if (_enemyUnits.Remove(u) && _enemyUnits.Count == 0)
            OnEnemyListEmpty?.Invoke();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}