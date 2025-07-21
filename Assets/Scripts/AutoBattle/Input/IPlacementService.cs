using UnityEngine;

namespace AutoBattle.Input
{
    /// <summary>
    /// 드래그된 유닛을 벤치 또는 필드 상의 셀로 배치하기 위한 서비스 인터페이스입니다.
    /// PlacementService 구현체를 통해 구체적인 배치/검증 로직을 제공합니다.
    /// </summary>
    public interface IPlacementService
    {
        /// <summary>
        /// 드래그된 객체를 최종 드롭 위치(cell)에 배치합니다.
        /// </summary>
        /// <param name="draggable">드래그 대상 유닛 인터페이스</param>
        /// <param name="targetCell">드롭된 셀의 좌표(Vector2Int)</param>
        void ProcessDrop(IUnitDraggable draggable, Vector2Int targetCell);
    }
}
