using UnityEngine;

namespace AutoBattle.Input
{
    public interface IUnitDraggable
    {
        /// <summary>이 슬롯/필드에 속한 실제 유닛 게임 오브젝트</summary>
        Unit Unit { get; }

        bool CanDrag { get; }

        IGridManager CurrentGrid { get; }

        Vector3 OriginalPosition { get; }

        /// <summary>
        /// 드래그가 시작될 때 호출됩니다.
        /// 예: 원위치 저장, 레이어 변경, 애니메이션 트리거 등.
        /// </summary>
        void OnDragBegin();

        /// <summary>
        /// 드래그 중 마우스 위치(worldPos)에 맞춰 호출됩니다.
        /// 예: 트랜스폼 포지션 업데이트, 하이라이트 표시 등.
        /// </summary>
        /// <param name="worldPos">현재 마우스가 가리키는 월드 좌표</param>
        void OnDrag(Vector3 worldPos);

        /// <summary>
        /// 드래그가 끝나고 드롭 위치(cell)에 해당하는 셀 좌표가 결정되면 호출됩니다.
        /// 예: 최종 위치로 스냅, 효과 재생 등.
        /// </summary>
        /// <param name="finalCell">드롭된 셀의 그리드 좌표</param>
        void OnDragEnd(Vector2Int finalCell);

        /// <summary>
        /// 드랍이 유효하지 않아 원위치로 되돌려야 할 때 호출합니다.
        /// 예: 부드러운 트윈 백 애니메이션.
        /// </summary>
        void Revert();

        void SetCurrentGrid(IGridManager grid);
    }
}