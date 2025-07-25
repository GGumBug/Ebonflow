using UnityEngine;

public class UnitModelOffsetAdjuster
{
    [Tooltip("메인 카메라가 아니라면 참조를 지정")]
    public Camera _targetCamera = null;

    [Tooltip("기준이 되는 y 위치 (예: 0일 때 스케일=1)")]
    public float _referenceY = 0f;

    [Tooltip("스프라이트가 referenceScale 크기로 보일 때의 카메라 forward 축상 거리\nSetup 시 referenceY 위치에서 자동 계산됨")]
    public float _referenceDistance = 8.8f;

    [Tooltip("기준 거리에서의 로컬 스케일")]
    public Vector3 _referenceScale = Vector3.one;

    [Tooltip("카메라와 UnitModel의 X/Y 차이 1당 적용될 오프셋 가중치")]
    public float _offsetWeight = 0.01f;

    [Tooltip("카메라와 Collider의 거리가 referenceDistance에서 벗어날 때 Y축 오프셋 변화량 단위 가중치")]
    public float _CollideroffsetY = -0.02264f;

    private SpriteRenderer _modelSprite;
    private Vector3 _referenceLocalPosition;

    private Camera TargetCamera => _targetCamera ?? Camera.main;

    public void Setup(SpriteRenderer modelSprite)
    {
        _modelSprite = modelSprite;
        _referenceScale = modelSprite.transform.localScale;
        _referenceLocalPosition = modelSprite.transform.localPosition;

        InitReferenceDistance();
    }

    private void InitReferenceDistance()
    {
        Vector3 spritePos = _modelSprite.transform.position;
        Vector3 referencePos = new Vector3(spritePos.x, _referenceY, spritePos.z);
        Vector3 camForward = TargetCamera.transform.forward;
        Vector3 toReference = referencePos - TargetCamera.transform.position;

        _referenceDistance = Mathf.Max(Vector3.Dot(toReference, camForward), 0.01f);
    }

    public void CalculateDistanceBasedScaleOffset()
    {
        if (_modelSprite == null || TargetCamera == null)
            return;

        ApplyScale();
        ApplyOffset();
    }

    private void ApplyScale()
    {
        Vector3 toSprite = _modelSprite.transform.position - TargetCamera.transform.position;
        float distance = Mathf.Max(Vector3.Dot(toSprite, TargetCamera.transform.forward), 0.01f);

        float scaleFactor = _referenceDistance / distance;
        _modelSprite.transform.localScale = _referenceScale * scaleFactor;
    }

    private void ApplyOffset()
    {
        Vector3 spriteWorldPos = _modelSprite.transform.position;
        Vector3 camPos = TargetCamera.transform.position;

        float xOffset = GetAxisOffset(spriteWorldPos.x, camPos.x);
        float yOffset = GetAxisOffset(spriteWorldPos.y, camPos.y);

        _modelSprite.transform.localPosition = new Vector3(
            _referenceLocalPosition.x + xOffset,
            _referenceLocalPosition.y + yOffset,
            _referenceLocalPosition.z
        );
    }

    private float GetAxisOffset(float spriteAxis, float camAxis)
    {
        return (spriteAxis - camAxis) * _offsetWeight;
    }
}
