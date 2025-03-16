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

    private HashSet<Unit> _enemyUnits;

    public event Func<TeamType> OnRequestTeamType;
    public event Action OnEnemyListEmpty;

    public bool HasEnemies() => _enemyUnits != null && _enemyUnits.Count > 0;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        col.radius = detectionRadius;
        _enemyUnits = new HashSet<Unit>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Unit>(out Unit otherUnit))
        {
            if (otherUnit.GetTeam() != OnRequestTeamType.Invoke())
            {
                _enemyUnits.Add(otherUnit);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Unit>(out Unit otherUnit))
        {
            if (otherUnit.GetTeam() != OnRequestTeamType.Invoke() && _enemyUnits.Contains(otherUnit))
            {
                bool removed = _enemyUnits.Remove(otherUnit);

                if (!removed)
                    Debug.LogWarning($"_enemyUnits에서 {otherUnit.name} 제거에 실패했습니다.");

                if (_enemyUnits.Count <= 0)
                    OnEnemyListEmpty?.Invoke();
            }
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


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}