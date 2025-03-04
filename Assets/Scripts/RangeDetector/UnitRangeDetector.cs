using UnityEngine;

public class UnitRangeDetector : RangeDetector
{
    public bool IsEnemyInRange()
    {
        return FindEnemyInRange<Unit>() != null;
    }

    public T FindEnemyInRange<T>() where T : Unit
    {
        var colliders = GetObjectsInRange();
        if (colliders != null && colliders.Length > 0)
        {
            var closestObject = GetClosestObjectInRange(colliders);
            if (closestObject != null && closestObject.TryGetComponent<Unit>(out Unit colUnit))
            {
                Unit myUnit = GetComponent<Unit>();
                if (myUnit == null)
                {
                    Debug.LogWarning("This GameObject does not have a Unit component attached.");
                    return null;
                }
                if (myUnit.Team != colUnit.Team)
                    return colUnit as T;
            }
        }
        return null;
    }
}
