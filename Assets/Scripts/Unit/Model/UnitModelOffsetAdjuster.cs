using UnityEngine;

public class UnitModelOffsetAdjuster
{
    [Tooltip("메인 카메라가 아니라면 참조를 지정")]
    public Camera targetCamera = null;

    [Tooltip("기준이 되는 y 위치 (예: 0일 때 스케일=1)")]
    public float referenceY = 0f;

    [Tooltip("스프라이트가 referenceScale 크기로 보일 때의 카메라 forward 축상 거리\n" +
             "Setup/Awake 시 referenceY 위치에서 자동 계산됨")]
    public float referenceDistance = 8.8f;

    [Tooltip("기준 거리에서의 로컬 스케일")]
    public Vector3 referenceScale = Vector3.one;

    private SpriteRenderer modelSprite;

    /// <summary>
    /// 외부에서 SpriteRenderer를 넘겨받고,
    /// referenceScale과 referenceDistance를 referenceY 기준으로 초기화합니다.
    /// </summary>
    public void Setup(SpriteRenderer modelSprite)
    {
        this.modelSprite = modelSprite;
        targetCamera = targetCamera ?? Camera.main;
        referenceScale = modelSprite.transform.localScale;
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
    /// 카메라-Unit 거리 대비 상대 스케일을 계산해 적용
    /// </summary>
    public void CalculateDistanceBasedScaleOffset()
    {
        if (modelSprite == null || targetCamera == null)
            return;

        // 현재 y 위치를 포함해 forward 축상 거리 계산
        float d = Vector3.Dot(
            modelSprite.transform.position - targetCamera.transform.position,
            targetCamera.transform.forward
        );
        d = Mathf.Max(d, 0.01f);

        // referenceDistance 대비 비율 적용
        float scaleFactor = referenceDistance / d;
        modelSprite.transform.localScale = referenceScale * scaleFactor;
    }
}
