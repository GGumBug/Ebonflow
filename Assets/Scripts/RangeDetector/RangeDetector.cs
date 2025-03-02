using UnityEngine;

public class RangeDetector : MonoBehaviour, IRangeDetector
{
    [Header("Detection Settings")]
    [Tooltip("월드 단위의 감지 반지름입니다.")]
    [SerializeField] private float detectionRadius = 5f;

    [Tooltip("감지할 오브젝트의 레이어 마스크입니다.")]
    [SerializeField] private LayerMask detectionLayer;

    private Collider2D[] GetObjectsInRange()
    {
        return Physics2D.OverlapCircleAll(transform.position, detectionRadius, detectionLayer);
    }

    public bool IsOtherObjectInRange()
    {
        var colliders = GetObjectsInRange();
        foreach (var col in colliders)
        {
            if (col.gameObject != gameObject)
                return true;
        }

        return false;
    }

    public GameObject GetClosestObjectInRange()
    {
        Collider2D[] colliders = GetObjectsInRange();
        GameObject closestObject = null;
        float minDistance = Mathf.Infinity;

        foreach (var col in colliders)
        {
            if (col.gameObject == gameObject)
                continue;

            float distance = Vector2.Distance(transform.position, col.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestObject = col.gameObject;
            }
        }

        return closestObject;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
