using UnityEngine;

public class UnitModel : MonoBehaviour
{
    private SpriteRenderer _modelSprite;

    public UnitModelOffsetAdjuster ModelOffsetAdjuster { get; private set; }

    private void Awake()
    {
        _modelSprite = GetComponent<SpriteRenderer>();
        ModelOffsetAdjuster = new UnitModelOffsetAdjuster();
        ModelOffsetAdjuster.Setup(_modelSprite);
    }

    private void LateUpdate()
    {
        ModelOffsetAdjuster.CalculateDistanceBasedScaleOffset();
    }
}
