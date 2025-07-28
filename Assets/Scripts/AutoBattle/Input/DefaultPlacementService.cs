using System;
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
            if ((fieldGrid == null) || (benchGrid == null))
                Debug.LogError("IGridManager가 할당되지 않았습니다.");

            _fieldGrid = fieldGrid;
            _benchGrid = benchGrid;
        }

        public bool ProcessDrop(IUnitDraggable draggable, Vector2Int targetCell)
        {
            // 1) 어떤 Grid로 드롭하려는가?
            IGridManager targetGrid = null;
            if (_fieldGrid.IsValidCell(targetCell)) targetGrid = _fieldGrid;
            else if (_benchGrid.IsValidCell(targetCell)) targetGrid = _benchGrid;

            if (targetGrid == null)
            {
                Debug.LogWarning($"No valid grid for {targetCell}, reverting.");
                draggable.Revert();
                return false;
            }

            if (!targetGrid.CanDrop)
            {
                Debug.LogWarning($"Grid is can not Drop, reverting.");
                draggable.Revert();
                return false;
            }

            // 2) 타겟 셀 사용 가능 여부
            if (targetGrid.IsCellOccupied(targetCell))
            {
                Debug.LogWarning($"{targetGrid} cell {targetCell} is occupied.");
                draggable.Revert();
                return false;
            }

            if (draggable.CurrentGrid.Type != targetGrid.Type)
            {
                Vector2Int originPos = new Vector2Int(Mathf.RoundToInt(draggable.OriginalPosition.x), Mathf.RoundToInt(draggable.OriginalPosition.y));
                draggable.CurrentGrid.RemoveUnit(originPos, draggable.Unit);
            }

            // 4) 배치 시도
            try
            {
                targetGrid.PlaceUnit(draggable, targetCell);
                draggable.Unit.SetCurrentGrid(targetGrid);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Place failed: {e.Message}, reverting.");
                draggable.Revert();
                return false;
            }
        }
    }
}
