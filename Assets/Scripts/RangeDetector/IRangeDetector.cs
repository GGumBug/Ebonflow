using UnityEngine;

public interface IRangeDetector
{
    public bool IsOtherObjectInRange();

    public GameObject GetClosestObjectInRange();
}
