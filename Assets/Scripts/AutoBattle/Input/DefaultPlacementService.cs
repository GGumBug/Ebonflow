using UnityEngine;

namespace AutoBattle.Input
{
    /// <summary>
    /// 기본 PlacementService 구현체
    /// - FieldGrid와 BenchGrid를 통해 셀 유효성 검증 및 배치 처리
    /// - 유효하지 않거나 점유된 셀에 드롭 시 되돌리기(Revert)
    /// </summary>
    public class DefaultPlacementService : IPlacementService
    {
        private readonly IGridManager _fieldGrid;
        private readonly IGridManager _benchGrid;

        /// <param name="fieldGrid">전투 필드 그리드 매니저 (경계, 점유 상태 확인, 배치)</param>
        /// <param name="benchGrid">벤치 그리드 매니저 (경계, 점유 상태 확인, 배치)</param>
        public DefaultPlacementService(IGridManager fieldGrid, IGridManager benchGrid)
        {
            _fieldGrid = fieldGrid;
            _benchGrid = benchGrid;
        }

        public void ProcessDrop(IUnitDraggable draggable, Vector2Int targetCell)
        {
            // 필드 영역 내 유효한 셀인지 확인
            if (_fieldGrid.IsValidCell(targetCell))
            {
                if (!_fieldGrid.IsCellOccupied(targetCell))
                {
                    _fieldGrid.PlaceUnit(draggable, targetCell);
                    return;
                }
                Debug.LogWarning($"Field cell {targetCell} is already occupied.");
            }
            // 벤치 영역 내 유효한 셀인지 확인
            if (_benchGrid.IsValidCell(targetCell))
            {
                if (!_benchGrid.IsCellOccupied(targetCell))
                {
                    _benchGrid.PlaceUnit(draggable, targetCell);
                    return;
                }
                Debug.LogWarning($"Bench cell {targetCell} is already occupied.");
            }
            // 유효하지 않거나 점유된 셀인 경우 되돌리기
            Debug.LogWarning($"Cannot place unit at {targetCell}, reverting.");
            draggable.Revert();
        }
    }
}
