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

    [Tooltip("카메라와 UnitModel의 X축/Y축 차이 1당 적용될 오프셋 가중치")]
    public float _offsetWeight = 0.01f;

    [Tooltip("카메라와 Collider의 거리가 referenceDistance에서 벗어날 때 Y축 오프셋 변화량 단위 가중치")]
    public float _CollideroffsetY = -0.02264f;

    private SpriteRenderer _modelSprite;
    private Vector3 _referenceLocalPosition;

    public void Setup(SpriteRenderer modelSprite)
    {
        _modelSprite = modelSprite;
        _targetCamera = _targetCamera ?? Camera.main;

        _referenceScale = modelSprite.transform.localScale;
        _referenceLocalPosition = modelSprite.transform.localPosition;

        InitReferenceDistance();
    }

    private void InitReferenceDistance()
    {
        Vector3 refPos = new Vector3(
            _modelSprite.transform.position.x,
            _referenceY,
            _modelSprite.transform.position.z
        );

        float d = Vector3.Dot(
            refPos - _targetCamera.transform.position,
            _targetCamera.transform.forward
        );

        _referenceDistance = Mathf.Max(d, 0.01f);
    }

    public void CalculateDistanceBasedScaleOffset()
    {
        if (_modelSprite == null || _targetCamera == null)
            return;

        // 1) 거리 기반 스케일
        float d = Vector3.Dot(
            _modelSprite.transform.position - _targetCamera.transform.position,
            _targetCamera.transform.forward
        );
        d = Mathf.Max(d, 0.01f);

        float scaleFactor = _referenceDistance / d;
        _modelSprite.transform.localScale = _referenceScale * scaleFactor;

        // 2) 스프라이트 로컬 위치 오프셋
        Vector3 worldPos = _modelSprite.transform.position;
        float xDiff = worldPos.x - _targetCamera.transform.position.x;
        float yDiff = worldPos.y - _targetCamera.transform.position.y;

        float xOffset = xDiff * _offsetWeight;
        float yOffset = yDiff * _offsetWeight;

        _modelSprite.transform.localPosition = new Vector3(
            _referenceLocalPosition.x + xOffset,
            _referenceLocalPosition.y + yOffset,
            _referenceLocalPosition.z
        );
    }
}
