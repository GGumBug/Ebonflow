using UnityEngine;
using AutoBattle.Input;
using System;

public class UnitDragController : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private Vector3 origin = Vector3.zero;

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;

    [Header("Layers")]
    [SerializeField] private int draggableLayer = 12;

    private LayerMask DraggableMask => 1 << draggableLayer;

    private IPlacementInputGate _inputGate;
    private IPlacementService _placementService;
    private InputReader _reader;
    private IUnitDraggable _currentDraggable;

    public Action OnDragStartedAction;
    public Action OnDragEndedAction;

    /// <summary>
    /// 초기 설정: InputGate, PlacementService 주입 및 이벤트 구독
    /// </summary>
    public void Setup(IPlacementInputGate inputGate, IPlacementService placementService)
    {
        _inputGate = inputGate;
        _placementService = placementService;
        _reader = InputManager.Instance.Reader;
        mainCamera ??= Camera.main;

        _reader.OnSelectStarted += OnDragStarted;
        _reader.OnSelectDown += OnDragUpdated;
        _reader.OnSelectCanceled += OnDragEnded;
    }

    private bool CanProcess => _inputGate?.IsEnabled == true && _reader != null;

    private void OnDragStarted()
    {
        if (!CanProcess) return;

        Vector3 world3D = ScreenToWorld3D(_reader.MousePosition);
        Vector2 world2D = world3D;

        Collider2D col = Physics2D.OverlapPoint(world2D, DraggableMask);
        if (col == null) return;

        var draggable = col.GetComponentInParent<IUnitDraggable>();
        if (draggable != null)
        {
            if (!draggable.CanDrag)
                return;
            
            _currentDraggable = draggable;

            draggable.OnDragBegin();
            OnDragStartedAction?.Invoke();
        }
    }

    private void OnDragUpdated()
    {
        if (!CanProcess || _currentDraggable == null) return;

        Vector3 world3D = ScreenToWorld3D(_reader.MousePosition);

        _currentDraggable.OnDrag(world3D);
    }

    private void OnDragEnded()
    {
        if (!CanProcess || _currentDraggable == null) return;

        Vector3 world3D = ScreenToWorld3D(_reader.MousePosition);
        Vector2Int cell = WorldToCell(world3D);

        if (_placementService.ProcessDrop(_currentDraggable, cell))
            _currentDraggable.OnDragEnd(cell);

        _currentDraggable = null;
        OnDragEndedAction?.Invoke();
    }

    private Vector3 ScreenToWorld3D(Vector2 screenPos)
    {
        // 1) 카메라 회전값 계산
        Quaternion cameraRotation = mainCamera.transform.rotation;
        Vector3 cameraEuler = cameraRotation.eulerAngles;
        // (필요하다면 cameraRotation 또는 cameraEuler를 다른 로직에 활용)

        // 2) 스크린 좌표를 이용해 Ray 생성 (카메라 회전·투영 자동 반영)
        Ray ray = mainCamera.ScreenPointToRay(screenPos);

        // 3) 월드 Z=0 평면(월드 XY 평면) 정의
        Plane plane = new Plane(Vector3.forward, Vector3.zero);

        // 4) Ray–평면 교차 지점 계산
        if (plane.Raycast(ray, out float enter))
        {
            Vector3 worldPoint = ray.GetPoint(enter);
            return worldPoint;
        }

        // 평면과 교차하지 않을 경우 예외 처리
        Debug.LogWarning("ScreenToWorld3D: Ray did not hit the Z=0 plane.");
        return Vector3.zero;
    }


    /// <summary>
    /// 월드 좌표 → 그리드 셀 좌표 변환
    /// </summary>
    private Vector2Int WorldToCell(Vector3 worldPos)
    {
        Vector3 local = worldPos - origin;
        int x = Mathf.RoundToInt(local.x / tileSize);
        int y = Mathf.RoundToInt(local.y / tileSize);
        return new Vector2Int(x, y);
    }

    /// <summary>
    /// 외부 페이즈 전환 시 강제 드래그 취소
    /// </summary>
    public void ForceCancel()
    {
        if (_currentDraggable != null)
        {
            _currentDraggable.Revert();
            _currentDraggable = null;
        }
    }

    private void OnDisable()
    {
        if (_reader == null) return;

        _reader.OnSelectStarted -= OnDragStarted;
        _reader.OnSelectDown -= OnDragUpdated;
        _reader.OnSelectCanceled -= OnDragEnded;
    }
}