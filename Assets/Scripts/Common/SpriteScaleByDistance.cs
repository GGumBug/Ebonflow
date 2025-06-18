using UnityEngine;

public class SpriteScaleByDistance : MonoBehaviour
{
    [Tooltip("메인 카메라가 아니라면 참조를 지정")]
    public Camera targetCamera = null;

    [Tooltip("스프라이트가 referenceScale 크기로 보일 때의 카메라 forward 축상 거리")]
    public float referenceDistance = 8.8f; // 씬에서 y=0(또는 기준 y) 위치에 배치한 스프라이트를 기준으로 미리 측정

    [Tooltip("기준 거리에서의 로컬 스케일")]
    public Vector3 referenceScale = Vector3.one;

    private SpriteRenderer modelSprite;

    /// <summary>
    /// 외부에서 SpriteRenderer를 넘겨받고, 
    /// referenceScale은 현재 로컬 스케일로 초기화만 합니다.
    /// referenceDistance는 Inspector에서 수치로 고정 설정하세요.
    /// </summary>
    public void Setup(SpriteRenderer modelSprite)
    {
        this.modelSprite = modelSprite;
        targetCamera = targetCamera ?? Camera.main;
        referenceScale = modelSprite.transform.localScale;
    }

    void LateUpdate()
    {
        if (modelSprite == null || targetCamera == null)
            return;

        // 카메라 forward 방향(정면 축)을 따라 떨어진 거리(d)를 구함
        float d = Vector3.Dot(
            modelSprite.transform.position - targetCamera.transform.position,
            targetCamera.transform.forward
        );
        d = Mathf.Max(d, 0.01f);

        // 기준 거리 대비 비율 계산
        float scaleFactor = referenceDistance / d;
        modelSprite.transform.localScale = referenceScale * scaleFactor;
    }
}
