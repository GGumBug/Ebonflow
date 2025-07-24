using UnityEngine;
using AutoBattle.Input;
using System.Collections;
using System;

[RequireComponent(typeof(Unit))]
public class UnitDraggableBehaviour : MonoBehaviour, IUnitDraggable
{
    public Unit Unit { get; private set; }

    // 드래그 전 원위치·부모 저장
    private Transform _originalParent;
    // 애니메이션 코루틴 핸들
    private Coroutine _revertCoroutine;
    private IGridManager _gridManager;

    public Vector3 OriginalPosition { get; private set; }

    public IGridManager CurrentGrid => _gridManager;

    public void Setup(Unit unit, IGridManager gridManager)
    {
        Unit = unit;
        _gridManager = gridManager;

        _originalParent = transform.parent;
    }

    public void OnDragBegin()
    {
        // 원위치 및 부모 저장
        OriginalPosition = transform.position;
        _originalParent = transform.parent;
    }

    public void OnDrag(Vector3 worldPos)
    {
        // 마우스 위치로 따라다니기
        transform.position = new Vector3(worldPos.x, worldPos.y, transform.position.z);
    }

    public void OnDragEnd(Vector2Int finalCell)
    {

    }

    public void Revert()
    {
        // 이미 실행 중인 복귀 애니메이션이 있으면 정지
        if (_revertCoroutine != null)
            StopCoroutine(_revertCoroutine);

        // 부드러운 트윈 애니메이션 예시
        _revertCoroutine = StartCoroutine(RevertRoutine(0.2f));
    }

    private IEnumerator RevertRoutine(float duration)
    {
        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Transform startParent = transform.parent;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector3.Lerp(startPos, OriginalPosition, t);
            yield return null;
        }

        // 애니메이션 끝나면 원래 부모 복원
        transform.SetParent(_originalParent, true);
        transform.position = OriginalPosition;
        _revertCoroutine = null;
    }

    public void SetCurrentGrid(IGridManager grid)
    {
        _gridManager = grid;
    }
}
