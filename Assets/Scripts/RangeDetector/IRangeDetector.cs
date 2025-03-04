using UnityEngine;

public interface IRangeDetector
{
    bool IsOtherObjectInRange();

    GameObject GetClosestObjectInRange(Collider2D[] colliders);
}
