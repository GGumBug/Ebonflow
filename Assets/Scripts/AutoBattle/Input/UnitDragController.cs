using UnityEngine;

namespace AutoBattle.Input
{
    public class UnitDragController
    {
        [SerializeField] private float tileSize = 1f;
        [SerializeField] private Vector3 origin = Vector3.zero;
        [SerializeField] private Camera mainCamera;

        private IPlacementInputGate _inputGate;
        private IPlacementService _placementService;
        private IUnitDraggable _currentDraggable;
        private InputReader _reader;

        /// <summary>
        /// 초기 설정: 입력 게이트와 PlacementService 주입, 카메라 할당, 이벤트 구독
        /// </summary>
        public void Setup(IPlacementInputGate inputGate, IPlacementService placementService)
        {
            _inputGate = inputGate;
            _placementService = placementService;
            _reader = InputManager.Instance.Reader;

            // 카메라 주입 또는 MainCamera 사용
            mainCamera ??= Camera.main;

            // 입력 이벤트 구독
            _reader.OnSelectStarted += TryBeginDrag;
            _reader.OnSelectPerformed += UpdateDrag;
            _reader.OnSelectCanceled += EndDrag;
        }

        /// <summary>
        /// 입력 처리 가능 여부
        /// </summary>
        private bool CanProcess => _inputGate?.IsEnabled == true && mainCamera != null;

        private void TryBeginDrag()
        {
            if (!CanProcess)
                return;

            var ray = mainCamera.ScreenPointToRay(_reader.MousePosition);
            if (Physics.Raycast(ray, out var hit, 100f) && hit.collider.TryGetComponent<IUnitDraggable>(out var draggable))
            {
                _currentDraggable = draggable;
                _currentDraggable.OnDragBegin();
            }
        }

        private void UpdateDrag()
        {
            if (!CanProcess || _currentDraggable == null)
                return;

            var plane = new Plane(Vector3.up, origin);
            var ray = mainCamera.ScreenPointToRay(_reader.MousePosition);
            if (plane.Raycast(ray, out var enter))
            {
                var worldPos = ray.GetPoint(enter);
                _currentDraggable.OnDrag(worldPos);
            }
        }

        private void EndDrag()
        {
            if (!CanProcess || _currentDraggable == null)
                return;

            var cell = ScreenPointToCell(_reader.MousePosition);
            _placementService.ProcessDrop(_currentDraggable, cell);
            _currentDraggable.OnDragEnd(cell);
            _currentDraggable = null;
        }

        /// <summary>
        /// 외부에서 페이즈 전환 시 강제 취소
        /// </summary>
        public void ForceCancelIfAny()
        {
            if (_currentDraggable != null)
            {
                _currentDraggable.Revert();
                _currentDraggable = null;
            }
        }

        /// <summary>
        /// 스크린 좌표를 셀 좌표로 변환
        /// </summary>
        public Vector2Int ScreenPointToCell(Vector2 screenPoint)
        {
            if (mainCamera == null)
                return new Vector2Int(-1, -1);

            var ray = mainCamera.ScreenPointToRay(screenPoint);
            var plane = new Plane(Vector3.up, origin);
            if (plane.Raycast(ray, out var enter))
                return WorldToCell(ray.GetPoint(enter));

            return new Vector2Int(-1, -1);
        }

        /// <summary>
        /// 월드 좌표를 셀 좌표로 변환
        /// </summary>
        public Vector2Int WorldToCell(Vector3 worldPos)
        {
            var local = worldPos - origin;
            int cellX = Mathf.RoundToInt(local.x / tileSize);
            int cellY = Mathf.RoundToInt(local.z / tileSize);
            return new Vector2Int(cellX, cellY);
        }

        public void OnDisableEvents()
        {
            if (_reader != null)
            {
                _reader.OnSelectStarted -= TryBeginDrag;
                _reader.OnSelectPerformed -= UpdateDrag;
                _reader.OnSelectCanceled -= EndDrag;
            }
        }
    }
}