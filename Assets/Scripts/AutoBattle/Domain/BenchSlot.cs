using System;

public class BenchSlot
{
    public int Index { get; }
    public Unit Unit { get; private set; }

    public bool IsLocked { get; private set; }

    public event Action<BenchSlot, Unit> OnUnitSet;
    public event Action<BenchSlot, Unit> OnUnitCleared;
    public event Action<BenchSlot, bool> OnLockChanged;

    public bool IsEmpty => Unit == null;

    public BenchSlot(int index)
    {
        Index = index;
    }

    public bool TrySet(Unit unit)
    {
        if (unit == null) return false;
        if (!IsEmpty) return false;
        if (IsLocked) return false;

        Unit = unit;
        OnUnitSet?.Invoke(this, unit);
        return true;
    }

    public bool TryClear()
    {
        if (IsEmpty) return false;
        if (IsLocked) return false;

        var removed = Unit;
        Unit = null;
        OnUnitCleared?.Invoke(this, removed);
        return true;
    }

    public void SetLock(bool locked)
    {
        if (IsLocked == locked) return;
        IsLocked = locked;
        OnLockChanged?.Invoke(this, locked);
    }

    /// <summary>
    /// 이벤트를 발생시키지 않고 Unit 참조만 교체 (벤치 내부 스왑 전용)
    /// </summary>
    internal void SetUnitRaw(Unit unit)
    {
        Unit = unit;
    }
}
