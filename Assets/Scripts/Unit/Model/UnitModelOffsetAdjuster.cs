using UnityEngine;

public class UnitModelOffsetAdjuster
{
    [Tooltip("메인 카메라가 아니라면 참조를 지정")]
    public Camera targetCamera = null;

    [Tooltip("기준이 되는 y 위치 (예: 0일 때 스케일=1)")]
    public float referenceY = 0f;

    [Tooltip("스프라이트가 referenceScale 크기로 보일 때의 카메라 forward 축상 거리\nSetup 시 referenceY 위치에서 자동 계산됨")]
    public float referenceDistance = 8.8f;

    [Tooltip("기준 거리에서의 로컬 스케일")]
    public Vector3 referenceScale = Vector3.one;

    [Tooltip("카메라와 UnitModel의 X축/Y축 차이 1당 적용될 오프셋 가중치")]
    public float offsetWeight = 0.01f;

    [Tooltip("Y축 기본 오프셋 값")]
    public float baseYOffset = 0.75f;

    private SpriteRenderer modelSprite;
    private Vector3 referenceLocalPosition;

    /// <summary>
    /// 외부에서 SpriteRenderer를 넘겨받고,
    /// referenceScale, referenceLocalPosition과 referenceDistance를 referenceY 기준으로 초기화합니다.
    /// </summary>
    public void Setup(SpriteRenderer modelSprite)
    {
        this.modelSprite = modelSprite;
        targetCamera = targetCamera ?? Camera.main;
        referenceScale = modelSprite.transform.localScale;
        referenceLocalPosition = modelSprite.transform.localPosition;
        InitReferenceDistance();
    }

    /// <summary>
    /// referenceY 높이에서 카메라 forward 축상 거리를 계산해 referenceDistance에 저장
    /// </summary>
    private void InitReferenceDistance()
    {
        Vector3 refPos = new Vector3(
            modelSprite.transform.position.x,
            referenceY,
            modelSprite.transform.position.z
        );

        float d = Vector3.Dot(
            refPos - targetCamera.transform.position,
            targetCamera.transform.forward
        );

        referenceDistance = Mathf.Max(d, 0.01f);
    }

    /// <summary>
    /// 카메라-Unit 거리 대비 상대 스케일을 계산해 적용하고,
    /// X/Y축 차이에 따른 위치 오프셋을 referenceLocalPosition 기준으로 적용합니다.
    /// </summary>
    public void CalculateDistanceBasedScaleOffset()
    {
        if (modelSprite == null || targetCamera == null)
            return;

        // 1) 거리 기반 스케일 계산
        float d = Vector3.Dot(
            modelSprite.transform.position - targetCamera.transform.position,
            targetCamera.transform.forward
        );
        d = Mathf.Max(d, 0.01f);

        float scaleFactor = referenceDistance / d;
        modelSprite.transform.localScale = referenceScale * scaleFactor;

        // 2) X/Y축 차이에 따른 위치 오프셋 적용
        Vector3 worldPos = modelSprite.transform.position;
        float xDiff = worldPos.x - targetCamera.transform.position.x;
        float yDiff = worldPos.y - targetCamera.transform.position.y;
        float xOffset = xDiff * offsetWeight;
        float yOffset = yDiff * offsetWeight;

        Vector3 newLocalPos = new Vector3(
            referenceLocalPosition.x + xOffset,
            referenceLocalPosition.y + yOffset,
            referenceLocalPosition.z
        );
        modelSprite.transform.localPosition = newLocalPos;
    }
}