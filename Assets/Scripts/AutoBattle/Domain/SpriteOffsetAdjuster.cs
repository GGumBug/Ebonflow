using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteOffsetAdjuster : MonoBehaviour
{
    [Header("Unit Custom Settings")]

    /// <summary>
    /// 카메라 시점 변화에 따른 X, Y축 위치 조정 가중치.
    /// Vector2(X축 가중치, Y축 가중치)
    /// </summary>
    public Vector2 OffsetWeightXY = new Vector2(0.01f, 0.01f);

    /// <summary>
    /// 깊이 변화(distance - referenceDistance)에 따른 X, Y축 위치 보정 값.
    /// 주로 Y축에만 필요하지만, X축도 조정 가능.
    /// Vector2(깊이 변화당 X 오프셋, 깊이 변화당 Y 오프셋)
    /// </summary>
    public Vector2 DepthOffsetXY = new Vector2(0f, -0.02264f);

    /// <summary>
    /// 이 유닛 스프라이트가 가질 기본 로컬 Y 위치 오프셋. (Manager의 Global 기준과 별개)
    /// </summary>
    public float BaseLocalYOffset = 0f;

    // 매니저가 참조할 초기 값들 (생략)
    public Vector3 InitialScale { get; private set; }
    public Vector3 InitialLocalPosition { get; private set; }

    // 초기 값들을 저장하는 공개 메서드 (매니저가 호출)
    public void StoreInitialValues()
    {
        InitialScale = transform.localScale;
        InitialLocalPosition = transform.localPosition;
    }

    void OnEnable()
    {
        // SpriteRenderer 대신 자기 자신(this)을 등록
        SpriteOffsetManager.Instance.Register(this);
    }

    void OnDisable()
    {
        // SpriteRenderer 대신 자기 자신(this)을 해제
        if (SpriteOffsetManager.Instance != null)
            SpriteOffsetManager.Instance.Unregister(this);
    }
}