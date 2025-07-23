using AutoBattle;
using AutoBattle.Input;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitBench : MonoBehaviour, IGridManager
{
    private const int BENCH_COUNT = 8;
    private const int BenchY = -2;
    private BenchSlot[] _slots;

    public int Capacity => _slots.Length;

    public GridType Type => GridType.Bench;

    /// <summary>벤치 단위 이벤트: 슬롯에 유닛 배치</summary>
    public event Action<UnitBench, BenchSlot, Unit> OnUnitPlaced;
    /// <summary>벤치 단위 이벤트: 슬롯 비워짐</summary>
    public event Action<UnitBench, BenchSlot, Unit> OnUnitRemoved;
    /// <summary>벤치 단위 이벤트: 슬롯 잠금 상태 변경</summary>
    public event Action<UnitBench, BenchSlot, bool> OnSlotLockChanged;

    private void Awake()
    {
        if (BENCH_COUNT <= 0)
            throw new ArgumentOutOfRangeException(nameof(BENCH_COUNT));

        _slots = new BenchSlot[BENCH_COUNT];
        for (int i = 0; i < BENCH_COUNT; i++)
        {
            var slot = new BenchSlot(i);
            // 슬롯 이벤트 → 벤치 이벤트로 재전파
            slot.OnUnitSet += HandleSlotUnitSet;
            slot.OnUnitCleared += HandleSlotUnitCleared;
            slot.OnLockChanged += HandleSlotLockChanged;
            _slots[i] = slot;
        }
    }

    #region 이벤트 재전파 핸들러
    private void HandleSlotUnitSet(BenchSlot slot, Unit unit)
        => OnUnitPlaced?.Invoke(this, slot, unit);

    private void HandleSlotUnitCleared(BenchSlot slot, Unit unit)
        => OnUnitRemoved?.Invoke(this, slot, unit);

    private void HandleSlotLockChanged(BenchSlot slot, bool locked)
        => OnSlotLockChanged?.Invoke(this, slot, locked);
    #endregion

    #region 조회
    public bool IsValidIndex(int index) => index >= 0 && index < Capacity;
    public BenchSlot GetSlot(int index)
        => IsValidIndex(index) ? _slots[index] : throw new IndexOutOfRangeException($"Index {index}");
    public Vector2Int GetBenchCell(int slotIndex)
        => new Vector2Int(slotIndex, BenchY);
    public Unit GetUnit(int index) => GetSlot(index).Unit;
    public bool IsEmpty(int index) => GetSlot(index).IsEmpty;
    public IEnumerable<BenchSlot> Slots()
    {
        for (int i = 0; i < Capacity; i++)
            yield return _slots[i];
    }
    #endregion

    #region 배치 / 제거
    /// <summary>
    /// 특정 인덱스에 유닛 배치 시도.
    /// </summary>
    public bool TryPlace(Unit unit, int index)
    {
        if (unit == null) return false;
        if (!IsValidIndex(index)) return false;
        return _slots[index].TrySet(unit);
    }

    /// <summary>
    /// 자동으로 첫 빈 슬롯을 찾아 배치. 성공 시 그 인덱스를 out.
    /// </summary>
    public bool TryPlaceFirstEmpty(Unit unit, out int placedIndex)
    {
        placedIndex = -1;
        if (unit == null) return false;
        for (int i = 0; i < Capacity; i++)
        {
            if (_slots[i].TrySet(unit))
            {
                placedIndex = i;
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 해당 인덱스에서 유닛 제거.
    /// </summary>
    public bool TryRemove(int index, out Unit removed)
    {
        removed = null;
        if (!IsValidIndex(index)) return false;
        return _slots[index].TryClear() && (removed = removed ?? null) == null
            ? AssignRemoved(index, out removed)
            : removed != null;
    }

    // TryClear 내에서 removed 유닛을 직접 받을 수 없으므로 별도 처리
    private bool AssignRemoved(int index, out Unit removed)
    {
        // 이벤트에서 이미 알림이 나갔기 때문에 여기선 null
        removed = null;
        return true;
    }

    /// <summary>
    /// 유닛 객체 참조를 직접 찾아 제거 (동일 레퍼런스 기준).
    /// </summary>
    public bool TryRemove(Unit unit)
    {
        if (unit == null) return false;
        for (int i = 0; i < Capacity; i++)
        {
            if (_slots[i].Unit == unit)
                return _slots[i].TryClear();
        }
        return false;
    }
    #endregion


    #region 스왑 / 이동
    public bool TrySwap(int indexA, int indexB)
    {
        if (indexA == indexB) return true;
        if (!IsValidIndex(indexA) || !IsValidIndex(indexB)) return false;

        var slotA = _slots[indexA];
        var slotB = _slots[indexB];

        if (slotA.IsLocked || slotB.IsLocked) return false;

        var uA = slotA.Unit;
        var uB = slotB.Unit;

        // 잠시 보호: 직접 교체 (이벤트 중복 방지를 위해 내부적으로 Set/Clear를 우회)
        slotA.SetUnitRaw(uB);
        slotB.SetUnitRaw(uA);

        // 수동으로 이벤트 재전파 (DirectSet 에서 이벤트 발생 안한다고 가정)
        if (uB != uA)
        {
            if (uB != null) OnUnitPlaced?.Invoke(this, slotA, uB);
            if (uA != null) OnUnitPlaced?.Invoke(this, slotB, uA);
        }
        return true;
    }

    /// <summary>
    /// A -> B 로 이동 (B가 비어 있어야 함)
    /// </summary>
    public bool TryMove(int from, int to)
    {
        if (from == to) return true;
        if (!IsValidIndex(from) || !IsValidIndex(to)) return false;

        var src = _slots[from];
        var dst = _slots[to];

        if (src.IsLocked || dst.IsLocked) return false;
        if (dst.Unit != null) return false;
        if (src.Unit == null) return false;

        var unit = src.Unit;
        src.SetUnitRaw(null);
        OnUnitRemoved?.Invoke(this, src, unit);

        dst.SetUnitRaw(unit);
        OnUnitPlaced?.Invoke(this, dst, unit);
        return true;
    }
    #endregion

    #region 락 / 초기화
    public void SetLock(int index, bool locked)
    {
        GetSlot(index).SetLock(locked);
    }

    public void UnlockAll()
    {
        for (int i = 0; i < Capacity; i++)
            _slots[i].SetLock(false);
    }

    public void ClearAll()
    {
        for (int i = 0; i < Capacity; i++)
            _slots[i].TryClear();
    }
    #endregion

    #region 유틸
    /// <summary>첫 빈 슬롯 인덱스. 없으면 -1.</summary>
    public int FirstEmptyIndex()
    {
        for (int i = 0; i < Capacity; i++)
            if (_slots[i].IsEmpty) return i;
        return -1;
    }

    public int OccupiedCount()
    {
        int c = 0;
        for (int i = 0; i < Capacity; i++)
            if (!_slots[i].IsEmpty) c++;
        return c;
    }
    #endregion

    #region IGridManager<Unit>
    public bool IsValidCell(Vector2Int cell)
    {
        // Y 좌표가 BenchY여야 하며, X는 슬롯 인덱스 범위 내여야 함
        return cell.y == BenchY && cell.x >= 0 && cell.x < Capacity;
    }

    public bool IsCellOccupied(Vector2Int cell)
    {
        if (!IsValidCell(cell))
            return false;
        return !_slots[cell.x].IsEmpty;
    }

    public void PlaceUnit(IUnitDraggable draggable, Vector2Int cell)
    {
        if (draggable == null)
            throw new ArgumentNullException(nameof(draggable));

        if (!IsValidCell(cell))
        {
            Debug.LogError($"PlaceUnit: 그리드 범위를 벗어난 셀 {cell}");
            return;
        }

        int index = cell.x;
        Vector3 originPos = draggable.OriginalPosition;
        Vector2Int originPosInt = new Vector2Int(Mathf.RoundToInt(originPos.x), Mathf.RoundToInt(originPos.y));

        PlaceUnitOrMove(draggable, originPosInt, cell, index);
    }

    public void PlaceUnitOrMove(IUnitDraggable draggable, Vector2Int originPosInt, Vector2Int cell, int index)
    {
        bool success = draggable.CurrentGrid.Type == Type
            ? TryMove(originPosInt.x, cell.x)
            : TryPlace(draggable.Unit, index);

        if (success)
        {
            draggable.Unit.SetSnapTransform(cell);
            return;
        }

        LogCannotPlace(index);
    }

    // 공통 로그 메서드
    private static void LogCannotPlace(int index)
    {
        Debug.LogWarning($"PlaceUnit: 슬롯 {index}에 유닛을 배치할 수 없습니다.");
    }

    public void RemoveUnit(IUnitDraggable draggable)
    {
        Vector3 originPos = draggable.OriginalPosition;
        Vector2Int originPosInt = new Vector2Int(Mathf.RoundToInt(originPos.x), Mathf.RoundToInt(originPos.y));

        Unit outUnit = null;
        TryRemove(originPosInt.x, out outUnit);
    }
    #endregion

    private void OnDrawGizmos()
    {
        if (_slots == null)
            return;

        // _grid 배열의 모든 셀을 순회
        for (int i = 0; i < _slots.Length; i++)
        {
            var slot = _slots[i];
            if (slot.IsEmpty)
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.red;
            }

             Gizmos.DrawWireSphere(new Vector2(i, BenchY), 0.4f);
        }
    }
}
