using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitBench
{
    private const int BenchY = -2;
    private readonly BenchSlot[] _slots;

    public int Capacity => _slots.Length;

    /// <summary>벤치 단위 이벤트: 슬롯에 유닛 배치</summary>
    public event Action<UnitBench, BenchSlot, Unit> OnUnitPlaced;
    /// <summary>벤치 단위 이벤트: 슬롯 비워짐</summary>
    public event Action<UnitBench, BenchSlot, Unit> OnUnitRemoved;
    /// <summary>벤치 단위 이벤트: 슬롯 잠금 상태 변경</summary>
    public event Action<UnitBench, BenchSlot, bool> OnSlotLockChanged;

    public UnitBench(int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity));

        _slots = new BenchSlot[capacity];
        for (int i = 0; i < capacity; i++)
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

}
