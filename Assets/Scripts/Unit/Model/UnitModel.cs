using UnityEngine;

public class UnitModel : MonoBehaviour
{
    private SpriteRenderer _modelSprite;
    private UnitModelOffsetAdjuster _unitModelOffsetAdjuster;

    private void Awake()
    {
        _modelSprite = GetComponent<SpriteRenderer>();
        _unitModelOffsetAdjuster = new UnitModelOffsetAdjuster();
        _unitModelOffsetAdjuster.Setup(_modelSprite);
    }

    private void LateUpdate()
    {
        _unitModelOffsetAdjuster.CalculateDistanceBasedScaleOffset();
    }
}
