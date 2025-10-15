using System.Collections.Generic;
using UnityEngine;

public class SpriteOffsetManager : Singleton<SpriteOffsetManager>, ILateUpdateObserver
{
    // ★ 전역 설정 값(offsetWeight 등) 제거. 이 값들은 Adjuster로 이동함.

    private Camera _targetCamera;
    private Vector3 _cameraForward;
    private Vector3 _cameraPosition;

    // 모든 유닛에 공통으로 적용되는 "기준 깊이 거리"
    private float _referenceDistance;

    // ★ 이 값은 Editor에서 설정해야 합니다. (모든 유닛의 기준 Y 높이)
    [SerializeField] private float _globalReferenceY = 0f;

    private readonly List<SpriteOffsetAdjuster> _adjusters = new List<SpriteOffsetAdjuster>();

    void Awake()
    {
        _targetCamera = Camera.main;

        // 기준 거리 초기화: (0, GlobalReferenceY, 0)을 기준으로 카메라와의 깊이 계산
        InitializeReferenceDistance();
    }

    private void InitializeReferenceDistance()
    {
        if (_targetCamera == null) return;

        Vector3 refPos = new Vector3(0, _globalReferenceY, 0);
        Vector3 toRef = refPos - _targetCamera.transform.position;

        // 초기 기준 거리를 계산하여 모든 스프라이트 조정의 공통 기준으로 사용
        _referenceDistance = Mathf.Max(Vector3.Dot(toRef, _targetCamera.transform.forward), 0.01f);
    }

    private void Start()
    {
        LateUpdateManager.Instance.RegisterObserver(this);
    }

    public void Register(SpriteOffsetAdjuster adjuster)
    {
        if (!_adjusters.Contains(adjuster))
        {
            // 등록 시 유닛별 초기 값 저장
            adjuster.StoreInitialValues();
            _adjusters.Add(adjuster);
        }
    }

    public void Unregister(SpriteOffsetAdjuster adjuster)
    {
        _adjusters.Remove(adjuster);
    }

    public void ObservedLateUpdate()
    {
        if (_targetCamera == null || _referenceDistance < Mathf.Epsilon) return;

        // 반복문 전에 카메라 데이터 캐시 (효율성)
        _cameraPosition = _targetCamera.transform.position;
        _cameraForward = _targetCamera.transform.forward;

        foreach (var adjuster in _adjusters)
        {
            // SpriteOffsetAdjuster 인스턴스와 공통 기준 데이터 전달
            AdjustSprite(adjuster);
        }
    }

    // AdjustSprite 로직은 이제 SpriteOffsetAdjuster의 데이터를 사용합니다.
    private void AdjustSprite(SpriteOffsetAdjuster adjuster)
    {
        Transform spriteT = adjuster.transform;

        // 1. 깊이(Distance) 계산
        Vector3 toSprite = spriteT.position - _cameraPosition;
        float distance = Mathf.Max(Vector3.Dot(toSprite, _cameraForward), 0.01f);

        // 2. 스케일 조정 (거리 기반 원근법)
        float scaleFactor = _referenceDistance / distance;
        spriteT.localScale = adjuster.InitialScale * scaleFactor;

        // 3. 위치 오프셋 계산
        float depthDelta = distance - _referenceDistance;

        float worldXDiff = spriteT.position.x - _cameraPosition.x;
        float worldYDiff = spriteT.position.y - _cameraPosition.y;

        // 1. 시점 오프셋 계산 (Vector2 사용)
        float xOffset = worldXDiff * adjuster.OffsetWeightXY.x;
        float yOffset = worldYDiff * adjuster.OffsetWeightXY.y;

        // 2. 깊이 변화에 따른 오프셋 추가 (Vector2 사용)
        xOffset += depthDelta * adjuster.DepthOffsetXY.x;
        yOffset += depthDelta * adjuster.DepthOffsetXY.y;

        // 3. 로컬 위치 적용 (BaseLocalYOffset 추가)
        spriteT.localPosition = new Vector3(
    adjuster.InitialLocalPosition.x + xOffset, // xOffset, yOffset이 월드 위치를 기반으로 계산됨
    adjuster.InitialLocalPosition.y + yOffset + adjuster.BaseLocalYOffset,
    adjuster.InitialLocalPosition.z
);
    }

    private void OnDisable()
    {
        if (LateUpdateManager.Instance != null)
            LateUpdateManager.Instance.UnRegisterObserver(this);
    }

    private void OnDestroy()
    {
        if (LateUpdateManager.Instance != null)
        {
            LateUpdateManager.Instance.UnRegisterObserver(this);
        }
    }
}
