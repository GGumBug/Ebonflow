using UnityEngine;
using AutoBattle.Input;

public class UnitDragController : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private Vector3 origin = Vector3.zero;

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;

    [Header("Layers")]
    [Tooltip("7번 레이어: 드래그 가능한 유닛")]
    [SerializeField] private int draggableLayer = 7;

    private LayerMask DraggableMask => 1 << draggableLayer;

    private IPlacementInputGate _inputGate;
    private IPlacementService _placementService;
    private InputReader _reader;
    private IUnitDraggable _currentDraggable;

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

    private bool CanProcess => _inputGate?.IsEnabled == true && _reader != null && mainCamera != null;

    private void OnDragStarted()
    {
        if (!CanProcess) return;

        Vector3 world3D = ScreenToWorld3D(_reader.MousePosition);
        Vector2 world2D = world3D;

        Debug.DrawRay(world3D, Vector3.zero, Color.green, 1f);

        // 클릭 지점에 드래그 가능한 유닛이 있는지 검사
        Collider2D col = Physics2D.OverlapPoint(world2D, DraggableMask);
        if (col != null && col.TryGetComponent<IUnitDraggable>(out var draggable))
        {
            _currentDraggable = draggable;
            draggable.OnDragBegin();
        }
    }

    private void OnDragUpdated()
    {
        if (!CanProcess || _currentDraggable == null) return;

        Vector3 world3D = ScreenToWorld3D(_reader.MousePosition);
        Debug.DrawRay(world3D, Vector3.zero, Color.yellow, 1f);

        _currentDraggable.OnDrag(world3D);
    }

    private void OnDragEnded()
    {
        if (!CanProcess || _currentDraggable == null) return;

        Vector3 world3D = ScreenToWorld3D(_reader.MousePosition);
        Vector2Int cell = WorldToCell(world3D);

        _placementService.ProcessDrop(_currentDraggable, cell);
        _currentDraggable.OnDragEnd(cell);
        _currentDraggable = null;
    }

    /// <summary>
    /// 화면상의 Vector2(mouse) → Z=0 평면 상의 월드 좌표(Vector3) 변환
    /// </summary>
    private Vector3 ScreenToWorld3D(Vector2 screenPos)
    {
        float zDistance = -mainCamera.transform.position.z;
        var screenPoint = new Vector3(screenPos.x, screenPos.y, zDistance);
        return mainCamera.ScreenToWorldPoint(screenPoint);
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